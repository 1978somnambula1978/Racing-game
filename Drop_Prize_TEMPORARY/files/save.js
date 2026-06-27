const fs = require('fs');

fs.writeFileSync(
nw.App.startPath + '/results.json',
JSON.stringify(
runtime.objects.JSON_results.getFirstInstance().getJsonDataCopy(),
null,
2
)
);