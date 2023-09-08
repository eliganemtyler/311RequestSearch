const fs = require('fs');
const path = require('path');

function updateRuntime(folder, hashEnd) {
  const runtimeFiles = fs.readdirSync(folder).filter(file => file.match(/^runtime.*\.js$/));
  const runtimeHash = runtimeFiles[0].split('.')[1];

  for (const file of fs.readdirSync(folder)) {
    if (!file.endsWith('.js')) {
      continue;
    }

    const name = path.parse(file).name;
    const oldHash = name.split('.')[1].replace(hashEnd, '');
    const newHash = oldHash + hashEnd;
    const runtimeFilePath = path.join(folder, `runtime.${runtimeHash}.js`);
    const runtimeFileContent = fs.readFileSync(runtimeFilePath, 'utf8');
    const newRuntimeFileContent = runtimeFileContent.replace(oldHash, newHash);
    fs.writeFileSync(runtimeFilePath, newRuntimeFileContent, 'utf8');
  }
}

function updateFileNames(folder, hashEnd) {
  for (const file of fs.readdirSync(folder)) {
    if (!file.endsWith('.js')) {
      continue;
    }

    const name = path.parse(file).name;
    const newName = `${name}${hashEnd}`;
    const filePath = path.join(folder, file);
    const indexFilePath = path.join(folder, 'index.html');
    const indexFileContent = fs.readFileSync(indexFilePath, 'utf8');
    const newIndexFileContent = indexFileContent.replace(name, newName);

    fs.renameSync(filePath, path.join(folder, `${newName}.js`));
    fs.writeFileSync(indexFilePath, newIndexFileContent, 'utf8');
  }
}

function runfix() {
  const jsonFile = 'angular.json';
  const jsonContent = fs.readFileSync(jsonFile, 'utf8');
  const projects = Object.keys(JSON.parse(jsonContent).projects);

  for (const project of projects) {
    const locales = JSON.parse(jsonContent).projects[project].i18n.locales;

    for (const key of Object.keys(locales)) {
      const folder = path.join("", `build/${key}`);
      const hashEnd = `-${key}`;

      updateFileNames(folder, hashEnd);
      updateRuntime(folder, hashEnd);
    }
  }
}

runfix();
