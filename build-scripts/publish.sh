#!/bin/sh
# POSIX

# This file is meant to be executed by the GitHub Actions workflow defined in /.github/workflows/publish.yml. It is
# not supported on TeamCity. See this for details on the build/versioning process:
# https://github.com/tyler-technologies/artifactory-github-migration/blob/master/05-ci/actions-samples/README.md

version=
publishContainer=false
publishArtifacts=false

artifactoryPullRegistry="$ARTIFACTORY_PULL_REGISTRY"
artifactoryPushRegistry="$ARTIFACTORY_PUSH_REGISTRY"
artifactoryUserName="$ARTIFACTORY_USERNAME"
artifactoryPassword="$ARTIFACTORY_PASSWORD"

if [ -n "$artifactoryUserName" ] && [ -n "$artifactoryPassword" ] && [ -z "$ARTIFACTORY_TOKEN" ]
then
  echo "setting ARTIFACTORY_TOKEN"
  export ARTIFACTORY_TOKEN="$(echo -n "$artifactoryUserName:$artifactoryPassword" | base64 -w 0)"
fi

# Parse arguments for flags, assumes unknown argument is version number
while :; do
  case $1 in
    --publish) # check for publish flag
      publishContainer=true
      shift
      ;;
    --publish-artifacts) # publish the output of the build, outside of a container
      publishArtifacts=true
      shift
      ;;
    --version) # version flag is required
      if [ "$2" ]; then
        version=$2
        shift
        shift
      else
        echo "No version provided"
        exit 1
      fi
      ;;
    --version=?*)
      version=${1#*=} # delete everything up to "=" and assign the remainder
      shift
      ;;
    --) # End of all options
      shift
      break
      ;;
    *) # no more options break out of loop
      break
  esac
done

# get the current tags of this commit
git fetch --tags

# default the version tag
versionTag="$version"

# validate version number
if [ "$(echo $version | grep -E "^[0-9]+\.[0-9]+.[0-9]+\$")" ]
then
  # The semantic versioning step looks for tags that match v[version]
  versionTag="v$version"
fi

echo "========== Building Version: $versionTag =========="

# get the script directory
scriptPath="$( cd "$(dirname "$0")" >/dev/null 2>&1 ; pwd -P )"

# go to the folder of the project source
cd $scriptPath/../311requestsearch

# create the tag name
dockerTag="$artifactoryPushRegistry/311requestsearch:$version"
echo "IMAGE_TAG=$dockerTag" >> "$GITHUB_ENV"

# log into docker registry
echo "Logging into registry $artifactoryPullRegistry"
docker login -u "$artifactoryUserName" -p "$artifactoryPassword" "$artifactoryPullRegistry"
if [ 0 != $? ]
then
  echo "Unable to log in to $artifactoryPullRegistry"
  exit 1
fi
echo "Logging into registry $artifactoryPushRegistry"
docker login -u "$artifactoryUserName" -p "$artifactoryPassword" "$artifactoryPushRegistry"
if [ 0 != $? ]
then
  echo "Unable to log in to $artifactoryPushRegistry"
  exit 1
fi

# build the container
echo "Building the code..."
chmod +x docker-build.sh
if [ "$GITHUB_ACTIONS" = "true" ]
then
  echo "Logging into registry tylertech-cache-docker-local.jfrog.io"
  docker login -u "$artifactoryUserName" -p "$artifactoryPassword" "tylertech-cache-docker-local.jfrog.io"

  if [ -n "$GITHUB_HEAD_REF" ]
  then
    curBranch=${GITHUB_HEAD_REF#refs/heads/}
  else
    curBranch=${GITHUB_REF#refs/heads/}
  fi

  curBranch=$(echo "$curBranch" | sed 's|/|-|g')

  dockerCacheResult="tylertech-cache-docker-local.jfrog.io/311requestsearch:$curBranch"
  if [ -n "$(curl -sS -u "$artifactoryUserName:$artifactoryPassword" 'https://tylertech.jfrog.io/artifactory/api/docker/cache-docker-local/v2/311requestsearch/tags/list' | jq -r '.tags[]' | grep "$curBranch-")" ]
  then
    dockerCacheSource="tylertech-cache-docker-local.jfrog.io/311requestsearch:$curBranch"
    echo "cached image found: $dockerCacheSource"
  else
    dockerCacheSource="tylertech-cache-docker-local.jfrog.io/311requestsearch:main"
    echo "no cached image for the current branch, using $dockerCacheSource"
  fi

  ./docker-build.sh -t "$dockerCacheResult-client" --build-arg CONFIGURATION=Release --build-arg "VERSION=$version" --target clientBuild --cache-from="$dockerCacheSource-client" --build-arg BUILDKIT_INLINE_CACHE=1 || echo "failed building stage clientBuild (cache: $dockerCacheSource-client)"
  ./docker-build.sh -t "$dockerCacheResult-server" --build-arg CONFIGURATION=Release --build-arg "VERSION=$version" --target netCoreBuild --cache-from="$dockerCacheSource-server" --build-arg BUILDKIT_INLINE_CACHE=1 || echo "failed building stage netCoreBuild (cache: $dockerCacheSource-server)"

  docker push "$dockerCacheResult-client" || echo "failed pushing cache image $dockerCacheResult-client"
  docker push "$dockerCacheResult-server" || echo "failed pushing cache image $dockerCacheResult-server"
fi

./docker-build.sh -t "$dockerTag" --build-arg CONFIGURATION=Release --build-arg "VERSION=$version"

# check to see if the docker build failed
if [ 0 != $? ]
then
  echo "$dockerTag build failed"
  exit 1
fi

# go back to the root folder
cd $scriptPath/..

# extract the built files from the container for build artifacts
if [ $publishArtifacts ]
then
  echo "Exporting artifacts..."
  docker create --name artifacts "$dockerTag"
  docker cp artifacts:/app $scriptPath/../_artifacts
  docker rm artifacts
else
  echo "Skipping the publishing of artifacts..."
fi

# check to see if container should be published
if [ $publishContainer = true ]
then
  # remove existing tag if necessary
  if [ -n "$(git tag -l "$versionTag")" ]
  then
    echo "removing existing tag \"$versionTag\""
    git tag -d "$versionTag"
    git push origin ":refs/tags/$versionTag"
  fi

  # add the new tag
  echo "adding tag \"$versionTag\" to origin"
  git tag -a "$versionTag" -m "$artifactoryPullRegistry/311requestsearch:$version" -m "https://tylertech.jfrog.io/ui/packages/docker:%2F%2F311requestsearch/$version"
  git push origin $versionTag

  # publish the container into the registry
  echo "Pushing container $dockerTag to the registry $artifactoryPushRegistry..."
  docker push "$dockerTag"
else
  echo "Skipping container publishing..."
fi

echo "Build complete"
