module.exports = exports = {
  "env": {
    "node": true
  },
  "root": true,
  "parserOptions": {},
  "ignorePatterns": [
    "projects/**/*"
  ],
  "overrides": [
    {
      "files": [
        "*.ts"
      ],
      "rules": {},
      "parserOptions": {
        "tsconfigRootDir": __dirname,
        "project": [
          "tsconfig.json",
          "e2e/tsconfig.json"
        ],
        "createDefaultProgram": true
      },
      "extends": [
        "plugin:@tylertech-eslint/angular/recommended",
        "plugin:@tylertech-eslint/recommended"
      ]
    },
    {
      "files": [
        "*.html"
      ],
      "extends": [
        "plugin:@tylertech-eslint/angular-template/recommended"
      ]
    }
  ]
}
