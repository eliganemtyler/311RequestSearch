#!/bin/bash
# Set up a GitHub repository. All arguments are optional - if missing the script will ask the user.
# Script dependencies:
#   - jq
#   - A GitHub personal access token with 'repo' and 'admin:org' scopes (https://github.com/settings/tokens)
#   - SSH is recommended (https://github.com/settings/keys)

set -e

SCRIPTPATH="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"
cd "$SCRIPTPATH"

usage()
{
  echo "[-t TOKEN] [-n 311RequestSearch] [--git-org tyler-technologies] [--team TEAM] [--http] [--art-team TEAM]"
  echo "Optional Arguments:"
  echo "-t TOKEN: GitHub personal access token"
  echo "-n 311RequestSearch: Repository name"
  echo "--git-org tyler-technologies: GitHub organization"
  echo "--team TEAM: GitHub team"
  echo "--http: Force http for remote"
  echo "--art-team: Artifactory team"
}

prompt_git_team()
{
  TEAM_CHOICES=$(get_team_choices)
  echo "Select a GitHub Team for this repository"
  echo -e "$TEAM_CHOICES" | cut -d $'\t' -f 2,3 --output-delimiter=': ' | nl -
  read -p "$(tput bold)> $(tput sgr0)" TEAM_INDEX

  if [[ "$TEAM_INDEX" =~ ^[0-9]+$ ]]
  then
    TEAM_LINE=$(echo -e "$TEAM_CHOICES" | sed "${TEAM_INDEX}q;d")
    if [ -z "$TEAM_LINE" ]
    then
      echo "$(tput setaf 1)Github team is required$(tput sgr0)"
      prompt_git_team
    else
      GIT_TEAM=$(echo -e "$TEAM_LINE" | cut -d $'\t' -f 1)
      GIT_TEAM_ID=$(echo -e "$TEAM_LINE" | cut -d $'\t' -f 4)
      get_parent_git_teams "$GIT_TEAM" "$TEAM_CHOICES"
    fi
  else
    # Teams are now required. I'm leaving the conditions to create repos without teams in place just in case we need to revert for
    # individual repos but I think it's safe to assume almost every repo should have a team.
    #GIT_TEAM_ID=""
    echo "$(tput setaf 1)Github team is required$(tput sgr0)"
    prompt_git_team
  fi
}

get_team_choices()
{
  RESPONSE=$(curl -sS -H "Authorization: token $GITHUB_TOKEN" -H "Content-Type: application/json" -X POST \
    --data-raw '{"query":"query { \n  organization(login: \"'"$GIT_ORG"'\") {\n    teams(first: 100, userLogins: [\"'"$GITHUB_USER"'\"]) {\n      totalCount,\n      nodes {\n        slug\n        name\n        description\n        databaseId\n        parentTeam {\n          slug\n          name\n        }\n      }\n    }\n  }\n}","variables":{}}' \
    'https://api.github.com/graphql')
  echo -e "$RESPONSE" | jq -r '.data.organization.teams.nodes[] | [.slug, .name, .description, .databaseId, .parentTeam.slug] | @tsv'
}

get_team_id()
{
  TEAM_CHOICES=$(get_team_choices)
  GIT_TEAM_ID=$(echo -e "$TEAM_CHOICES" | grep -E "^$1"$'\t' | cut -d $'\t' -f 4)
  
  if [ -z "$GIT_TEAM_ID" ]
  then
    echo "$(tput setaf 1)Unable to determine team id for $1. Either it is incorrect or you are not a member. Verify the team name is correct or remove the '--git-team' argument to select a team from a list.$(tput sgr0)"
    usage
    exit 1
  fi

  get_parent_git_teams "$1" "$TEAM_CHOICES"
}

get_parent_git_teams()
{
  if [ -z "$2" ]
  then
    TEAM_CHOICES=$(get_team_choices)
  else
    TEAM_CHOICES="$2"
  fi
  
  TEAM_SLUG="$1"
  GIT_PARENT_TEAMS=""
  while [ -n "$TEAM_SLUG" ]
  do
    TEAM_PARENT=$(echo -e "$TEAM_CHOICES" | grep -E "^$TEAM_SLUG"$'\t' | cut -d $'\t' -f 5)
    if [ -n "$TEAM_PARENT" ]
    then
      TEAM_SLUG="$TEAM_PARENT"
      GIT_PARENT_TEAMS="$GIT_PARENT_TEAMS $TEAM_SLUG"
    else
      TEAM_SLUG=""
    fi
  done
}

prompt_art_team()
{
  if [[ -z "$ARTIFACTORY_USERNAME" || -z "$ARTIFACTORY_PASSWORD" ]]
  then
    echo "$(tput setaf 1)Artifactory credentials are not configured.$(tput sgr0)"
    echo "https://github.com/tyler-technologies/artifactory-github-migration/blob/master/01-local-dev-setup/README.md"
    exit 1
  fi

  RESPONSE=$(curl -sS -u "$ARTIFACTORY_USERNAME:$ARTIFACTORY_PASSWORD" 'https://tylertech.jfrog.io/artifactory/api/repositories?type=local&packageType=nuget')
  if [ -n "$(echo -e "$RESPONSE" | grep '"errors"')" ]
  then
    echo -e "$RESPONSE"
    exit 1
  fi

  TEAMS=$(echo -e "$RESPONSE" | jq -r '.[].key' | cut -f 1 -d '-' | tr a-z A-Z | uniq)
  echo "Select your Artifactory team"
  echo -e "$TEAMS" | nl -v1 -
  read -p "$(tput bold)> $(tput sgr0)" TEAM_INDEX

  if [ -z "$TEAM_INDEX" ]
  then
    echo "$(tput setaf 1)No Artifactory team provided. You will have to update '.github/workflows/publish.yml' and '.github/workflows/pr-cleanup.yml' with the correct secrets manually.$(tput sgr0)"
  else
    ART_TEAM=$(echo -e "$TEAMS" | sed "${TEAM_INDEX}q;d")
  fi
}

while [ "$1" != "" ]
do
  case $1 in
    -t | --token ) shift
      GITHUB_TOKEN="$1"
      ;;
    --team ) shift
      GIT_TEAM="$1"
      ;;
    --art-team ) shift
      ART_TEAM="$1"
      ;;
    -n | --name ) shift
      REPO_NAME="$1"
      ;;
    --http ) FORCE_HTTP="true"
      ;;
    --git-org ) shift
      GIT_ORG="$1"
      ;;
    -h | --help ) usage
      exit
      ;;
    * ) usage
      exit 1
  esac
  shift
done

# Check dependencies
set +e
which jq > /dev/null
if [ "$?" != 0 ]
then
  echo "$(tput setaf 1)Missing dependency: jq$(tput sgr0)"
  echo "jq installation instructions: https://stedolan.github.io/jq/download/"
  exit 1
fi
set -e

# Exit if we already have a remote
if [[ -d ".git" && -n "$(git remote)" ]]
then
  echo "$(tput setaf 1)A remote already exists. If you are trying to migrate a project from BitBucket to GitHub, follow these steps: https://github.com/tyler-technologies/artifactory-github-migration$(tput sgr0)"
  exit 1
fi

if [ -z "$GIT_ORG" ]
then
  GIT_ORG="tyler-technologies"
fi

if [ -z "$GITHUB_TOKEN" ]
then
  echo "The GitHub API requires a token for authentication. You may generate a token here:"
  echo "https://github.com/settings/tokens"
  echo "Scopes should include 'repo' and 'admin:org'. Don't forget to Enable SSO for your token."
  echo "To prevent this prompt in the future either use the '-t' argument or set the GITHUB_TOKEN environment variable."
  read -p "$(tput bold)GitHub token> $(tput sgr0)" GITHUB_TOKEN
fi

RESPONSE=$(curl -sS -H "Authorization: token $GITHUB_TOKEN" 'https://api.github.com/user')
GITHUB_USER=$(echo "$RESPONSE" | jq -r '.login')
if [ "$GITHUB_USER" = "null" ]
then
  echo "$(tput setaf 1)Unable to get Github user. Your token may be invalid.$(tput sgr0)"
  echo "$RESPONSE"
  exit 1
fi

VALID="false"
while [ "$VALID" = "false" ]
do
  if [ -z "$REPO_NAME" ]
  then
    read -p "$(tput bold)Repository name [${PWD##*/}]> $(tput sgr0)" REPO_NAME
    if [ -z "$REPO_NAME" ]
    then
      REPO_NAME=${PWD##*/}
    fi
  fi

  if [[ "$REPO_NAME" =~ ^[a-zA-Z0-9]+(-[a-zA-Z0-9]+)*$ ]]
  then
    VALID="true"
  else
    echo "$(tput setaf 1)Invalid repository name: $REPO_NAME$(tput sgr0)"
    REPO_NAME=""
  fi
done

if [ -n "$GIT_TEAM" ]
then
  get_team_id "$GIT_TEAM"
else
  prompt_git_team
fi

if [ -n "$GIT_PARENT_TEAMS" ]
then
  VALID="false"
  while [ "$VALID" = "false" ]
  do
    echo "Give parent teams maintain permissions ($GIT_PARENT_TEAMS)?"
    read -p "[y/$(tput bold)N$(tput sgr0)]> " USE_PARENT_TEAMS
    if [[ "$USE_PARENT_TEAMS" = "y" || "$USE_PARENT_TEAMS" = "Y" ]]
    then
      VALID="true"
    elif [[ -z "$USE_PARENT_TEAMS" || "$USE_PARENT_TEAMS" = "n" || "$USE_PARENT_TEAMS" = "N" ]]
    then
      VALID="true"
      GIT_PARENT_TEAMS=""
    fi
  done
fi

if [ -z "$ART_TEAM" ]
then
  prompt_art_team
fi

# Print what we are going to do and give the option to cancel
echo -e "\n----------"
echo "Local: $PWD"
echo "Remote: $GIT_ORG / $REPO_NAME"
echo "GitHub team: $GIT_TEAM"
if [ -n "$GIT_PARENT_TEAMS" ]
then
  echo "Parent GitHub teams: $GIT_PARENT_TEAMS"
fi
echo "Artifactory team: $ART_TEAM"
if [ "$FORCE_HTTP" = "true" ]
then
  echo "Using HTTP for the remote. SSH is recommended: https://docs.github.com/en/github/authenticating-to-github/connecting-to-github-with-ssh"
else
  echo "Using SSH for the remote."
fi
echo "1_create_github_repository.sh will be deleted"
if [ -d ".git" ]
then
  echo "'$PWD' is already a git repository. $(tput setaf 1)If your repository contained secrets at any point in its history, exit this script now!$(tput sgr0)"
fi
read -p "$(tput bold)Continue? [Y,n]> $(tput sgr0)" c
if [[ "$c" != "y" && "$c" != "Y" && -n "$c" ]]
then
  exit 1
fi

# Create local repo
if [ -d ".git" ]
then
  echo "'$PWD' is already a git repository"
else
  echo "Initializing local repository..."
  git init
  git checkout -b main
  git add .
  git commit -m "Initial commit" -q
fi

# Create remote repo
echo "Creating Github repository..."
DATA="{\"name\":\"$REPO_NAME\", \"visibility\": \"private\""
if [ -n "$GIT_TEAM_ID" ]
then
  DATA="$DATA, \"team_id\":\"$GIT_TEAM_ID\""
fi
DATA="$DATA}"
RESPONSE=$(curl -sS -H "Authorization: token $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.nebula-preview+json" \
  --data "$DATA" \
  "https://api.github.com/orgs/$GIT_ORG/repos")
SSH_URL=$(echo -e "$RESPONSE" | jq -r '.ssh_url')

if [ "$SSH_URL" = "null" ]
then
  echo "$RESPONSE"
  exit 1
fi

echo "$(tput setaf 2)Repository created: https://github.com/$GIT_ORG/$REPO_NAME$(tput sgr0)"

if [ -n "$GIT_TEAM_ID" ]
then
  echo "Setting team permissions: $GIT_TEAM = admin"
  curl -sS -H "Authorization: token $GITHUB_TOKEN" \
    -X "PUT" \
    --data '{"permission": "admin"}' \
    "https://api.github.com/orgs/$GIT_ORG/teams/$GIT_TEAM/repos/$GIT_ORG/$REPO_NAME" > /dev/null

  # topics must only contain letters, numbers, or hyphens and be 35 chars or less.
  versionTopic="$(echo "tcp-web-armadillo-1.0.0-dev" | tr '.' '-' | cut -c -35)"
  echo "Setting repo topic: $GIT_TEAM, tcp-web-armadillo, $versionTopic"
  curl -sS -H "Authorization: token $GITHUB_TOKEN" \
    -H "Accept: application/vnd.github.mercy-preview+json" \
    -X "PUT" \
    --data '{"names": ["'"$GIT_TEAM"'", "tcp-web-armadillo", "'"$versionTopic"'"]}' \
    "https://api.github.com/repos/$GIT_ORG/$REPO_NAME/topics" > /dev/null
fi

if [ -n "$GIT_PARENT_TEAMS" ]
then
  echo "Setting parent team permissions"
  for team in $GIT_PARENT_TEAMS
  do
    echo "$team = maintain"
    curl -sS -H "Authorization: token $GITHUB_TOKEN" \
      -X "PUT" \
      --data '{"permission": "maintain"}' \
      "https://api.github.com/orgs/$GIT_ORG/teams/$team/repos/$GIT_ORG/$REPO_NAME" > /dev/null
  done
fi

echo "Enabling vunerability alerts"
curl -sS -H "Authorization: token $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.dorian-preview+json" \
  -X "PUT" \
  "https://api.github.com/repos/$GIT_ORG/$REPO_NAME/vulnerability-alerts" > /dev/null

echo "Enabling automated security fixes"
curl -sS -H "Authorization: token $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github.london-preview+json" \
  -X "PUT" \
  "https://api.github.com/repos/$GIT_ORG/$REPO_NAME/automated-security-fixes"

if [ "$FORCE_HTTP"]
then
  git remote add origin "https://github.com/$GIT_ORG/$REPO_NAME.git"
else
  git remote add origin "$SSH_URL"
fi

# Clean up
echo "Removing initialization scripts..."
git rm "$SCRIPTPATH/1_create_github_repository.sh"
git commit -m "chore: remove initialization scripts" -q

# Make build script executable
echo "Make build script executable..."
chmod +x build-scripts/publish.sh
git add build-scripts
# This is required for windows users.
git update-index --chmod=+x build-scripts/publish.sh
git commit -m "chore: make build script executable" -q || true

# Fix secrets in the actions workflow
if [ -n "$ART_TEAM" ]
then
  echo "Updating actions workflows with secret keys..."
  sed -i.bak 's/TEAM_ARTIFACTORY/'"$ART_TEAM"'_ARTIFACTORY/' .github/workflows/publish.yml
  sed -i.bak 's/TEAM_ARTIFACTORY/'"$ART_TEAM"'_ARTIFACTORY/' .github/workflows/pr-cleanup.yml
  rm .github/workflows/publish.yml.bak .github/workflows/pr-cleanup.yml.bak
  git add .github/workflows
  git commit -m "chore: set artifactory secrets" -q
else
  echo "$(tput setaf 1)Skipped setting artifactory secrets in the GitHub Actions workflows. You must set these manually for your builds to succeed.$(tput sgr0)"
fi

# Add a badge for GitHub Actions to the readme
echo "Adding build status badge to the readme..."
BADGE="![Build](https://github.com/$GIT_ORG/$REPO_NAME/workflows/Build/badge.svg)"
sed -i.bak '1s;^;'"$BADGE"'\n\n;' README.md
rm README.md.bak
git add README.md
git commit -m "chore: add build badge to readme" -q

# Add a CODEOWNERS if one doesn't exist
if [ ! -f ".github/CODEOWNERS" ]
then
  echo "Creating .github/CODEOWNERS..."
  echo "* @tyler-technologies/$GIT_TEAM" > .github/CODEOWNERS
  git add .github/CODEOWNERS
  git commit -m "chore: adding CODEOWNERS" -q
else
  echo ".github/CODEOWNERS already exists"
fi

echo "Pushing to remote..."
git push --set-upstream origin main

# Branch protection. We have to do this after the push so we don't get blocked.
echo "Setting branch protection..."
PAYLOAD='{
  "required_pull_request_reviews": {"dismiss_stale_reviews": true, "require_code_owner_reviews": true, "dismissal_restrictions": {"teams": ["'"$GIT_TEAM"'"]}},
  "enforce_admins": null,
  "required_status_checks": {"strict": true, "contexts": []},
  "restrictions": {"users": [], "teams": ["'"$GIT_TEAM"'"]}
}'
curl -sS -H "Authorization: token $GITHUB_TOKEN" \
    -X "PUT" \
    --data "$PAYLOAD" \
    "https://api.github.com/repos/$GIT_ORG/$REPO_NAME/branches/main/protection" > /dev/null
echo "$(tput setaf 2)Branch protection has been enabled for the main branch. There are a few settings you may wish to change in the future:"
echo "1. Include Administrators: Currently disabled. Enabling this setting will force team members to review each other's pull requests before merging, even if they are on a team with admin access to the repository."
echo "2. Required review count: Set to 1 by default."
echo "3. Github will automatically add reviewers to pull requests based on rules in your .github/CODEOWNERS file. If a CODEOWNERS was generated it only has one rule: '* @tyler-technologies/$GIT_TEAM'. You may wish to change this to make individuals responsible for specific areas of your application. Details: https://docs.github.com/en/github/creating-cloning-and-archiving-repositories/about-code-owners"
echo "Branch protection settings: https://github.com/$GIT_ORG/$REPO_NAME/settings/branches$(tput sgr0)"

echo -e "\n$(tput setaf 2)All done! Check the status of your build: https://github.com/$GIT_ORG/$REPO_NAME/actions$(tput sgr0)"
