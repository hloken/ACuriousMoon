#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

#addin "Cake.FileHelpers"
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/Example/bin") + Directory(configuration);

// Other variables
var db = "enceladus";
var build = Directory("./") + File("build.sql");
var scripts = Directory("./scripts");
var csv = Directory("./data/master_plan.csv");
var master = scripts + File("import.sql");
var normalize = scripts + File("normalize.sql");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("all")
    .IsDependentOn("normalize")
    .Does(() => 
    {
        StartAndReturnProcess("psql", new ProcessSettings { Arguments = $"{db} -f {build}"} );
    });

Task("master")
    .Does(()=> {
        FileAppendText(build, FileReadText(master))
    });

Task("import")
    .IsDependentOn("master")
    .Does(()=>{
        FileAppendText(build, $@"\COPY import.master_plan FROM {csv} WITH DELIMITER ',' HEADER CSV;")
    })

Task("normalize")
    .IsDependentOn("import")
    .Does(() => {
        FileAppendText(build, FileReadText(normalize))
    });

Task("clean")
    .Does(() => {
        CleanFile(build);
    });
//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("master");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
