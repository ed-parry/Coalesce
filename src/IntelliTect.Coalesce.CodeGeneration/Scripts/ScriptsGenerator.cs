﻿using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.CodeGeneration.Common;
using IntelliTect.Coalesce.Models;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Validation;
using System.Globalization;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.DotNet.ProjectModel;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    public class ScriptsGenerator : CommonGeneratorBase
    {
        public IModelTypesLocator ModelTypesLocator { get; }
        public IModelTypesLocator DataModelTypesLocator { get; }
        protected ICodeGeneratorActionsService CodeGeneratorActionsService { get; }

        public const string ScriptsFolderName = "Scripts";
        public const string ThisAssemblyName = "IntelliTect.Coalesce.CodeGeneration";

        private ProjectContext _webProject;

        public ScriptsGenerator(ProjectContext webProject, ProjectContext dataProject)
            : base(PlatformServices.Default.Application)
        {
            ModelTypesLocator = DependencyProvider.ModelTypesLocator(webProject);
            DataModelTypesLocator = DependencyProvider.ModelTypesLocator(dataProject);
            CodeGeneratorActionsService = DependencyProvider.CodeGeneratorActionsService(webProject);

            _webProject = webProject;
        }

        internal Task Generate(CommandLineGeneratorModel model)
        {
            Dictionary<string, Dictionary<int, string>> enumValues = new Dictionary<string, Dictionary<int, string>>();

            using (StreamWriter streamWriter = new StreamWriter("output.txt", false))
            {
                Console.WriteLine($"Starting Generator");
                string targetNamespace;
                if (!string.IsNullOrEmpty(model.TargetNamespace))
                {
                    targetNamespace = model.TargetNamespace;
                }
                else
                {
                    targetNamespace = ValidationUtil.ValidateType("Startup", "", ModelTypesLocator, throwWhenNotFound: false).Namespace;
                }
                Console.WriteLine($"Namespace: {targetNamespace}");

                ModelType dataContext = ValidationUtil.ValidateType(model.DataContextClass, "dataContext", DataModelTypesLocator, throwWhenNotFound: false);

                if (model.ValidateOnly)
                {
                    Console.WriteLine($"Validating model for: {dataContext.FullName}");
                }
                else
                {
                    Console.WriteLine($"Building scripts for: {dataContext.FullName}");
                }

                var models = ReflectionRepository
                                .AddContext((INamedTypeSymbol)dataContext.TypeSymbol)
                                .Where(m => m.PrimaryKey != null)
                                .ToList();

                var validationResult = ValidateContext.Validate(models);

                bool foundIssues = false;
                foreach (var validation in validationResult.Where(f => !f.WasSuccessful))
                {
                    foundIssues = true;
                    streamWriter.WriteLine(validation.ToString());
                    Console.WriteLine("--- " + validation.ToString());
                }
                if (!foundIssues)
                {
                    Console.WriteLine("Model validated successfully");
                }

                streamWriter.WriteLine($" {"Name",-15}  {"Type",-15}  {"Pure Type",-15} {"Col",-5} {"Array",-5} {"Key",-5} {"Complex",-7} {"DisplayName",-15} {"Null?",-5} {"Many",-5} {"Internal",-5} {"FileDL",-5} {"IsNum",-5} {"IsDT",-5} {"IsDTO",-5} {"IsBool",-5} {"IsStr",-5} {"IsEnum",-8} {"JsKoType",-25} {"TsKoType",-50} {"TsType",-15} {"DateOnly",-10} {"Hidden",-8} {"Required",-8} {"KeyName",-15} {"MinLength",-8} {"MaxLength",-10} {"Range",-10}");

                foreach (var obj in models.Where(p => p.HasDbSet))
                {
                    //Console.WriteLine($"{obj.Name}  dB:{obj.HasDbSet}");
                    streamWriter.WriteLine($"{obj.Name}  dB:{obj.HasDbSet}    Edit:{obj.IsEditAllowed}   Create:{obj.IsCreateAllowed}    Delete:{obj.IsDeleteAllowed}");

                    foreach (var prop in obj.Properties.Where(f => !f.IsInternalUse))
                    {
                        streamWriter.WriteLine($@" {prop.Name,-15}  {prop.TypeName,-15}  {prop.PureType.Name,-15} {prop.Type.IsCollection,-5} {prop.Type.IsArray,-5} {prop.IsPrimaryKey,-5} {prop.IsComplexType,-7} {prop.DisplayName,-15} {prop.Type.IsNullable,-5} {prop.IsManytoManyCollection,-5} {prop.IsInternalUse,-5}    {prop.IsFileDownload,-5}  {prop.Type.IsNumber,-5} {prop.Type.IsDateTime,-5} {prop.Type.IsDateTimeOffset,-5} {prop.Type.IsBool,-5}  {prop.Type.IsString,-5} {prop.Type.IsEnum,-8} {prop.Type.JsKnockoutType,-25} {prop.Type.TsKnockoutType,-50} {prop.Type.TsType,-15} {prop.IsDateOnly,-10} {prop.IsHidden(HiddenAttribute.Areas.Edit),-8}  {prop.IsRequired,-8} {prop.ObjectIdPropertyName,-15} {prop.MinLength,-8} {prop.MaxLength,-10} {prop.Range?.Item1 + " " + prop.Range?.Item2,-10}");
                        if (prop.Type.IsEnum && !enumValues.ContainsKey(prop.Name))
                        {
                            enumValues.Add(prop.Name, prop.Type.EnumValues);
                        }
                    }

                    foreach (var method in obj.Methods.Where(f => !f.IsInternalUse))
                    {
                        streamWriter.WriteLine($@" {method.Name,-15}  {method.ReturnType.Name,-15}  {method.ReturnType.PureType.Name,-15} {method.ReturnType.IsCollection,-5} {method.ReturnType.IsArray,-5} {null,-5} {null,-7} {method.DisplayName,-15} {method.ReturnType.IsNullable,-5} {null,-5} {null,-5}    {null,-5}  {method.ReturnType.IsNumber,-5} {method.ReturnType.IsDateTime,-5} {method.ReturnType.IsDateTimeOffset,-5} {method.ReturnType.IsBool,-5}  {method.ReturnType.IsString,-5} {method.ReturnType.IsEnum,-8} {method.ReturnType.JsKnockoutType,-25} {method.ReturnType.TsKnockoutType,-50} {method.ReturnType.TsType,-15} {null,-10} {method.IsHidden(HiddenAttribute.Areas.Edit),-8}  {null,-8} {null,-15} {null,-8} {null,-10} {"",-10}");
                    }

                }

                streamWriter.WriteLine("-------- Complex Types --------");
                //Console.WriteLine(TemplateFolders.First());
                foreach (var ct in ComplexTypes(models))
                {
                    streamWriter.WriteLine(ct.Name);
                    foreach (var prop in ct.Properties)
                    {
                        streamWriter.WriteLine($"    {prop.Name}");
                    }
                }

                streamWriter.WriteLine("-------- Enumerations --------");
                //Console.WriteLine(TemplateFolders.First());
                foreach (string propertyKey in enumValues.Keys)
                {
                    streamWriter.WriteLine(propertyKey);
                    foreach (var enumValue in enumValues[propertyKey])
                    {
                        streamWriter.WriteLine($"\t{enumValue.Key} : {enumValue.Value}");
                    }
                }

                if (foundIssues) throw new Exception("Model did not validate. " + validationResult.First(f => !f.WasSuccessful).ToString());

                if (model.ValidateOnly)
                {
                    return Task.FromResult(0);
                }
                else
                {
                    return GenerateScripts(model, models, dataContext, targetNamespace);
                }
            }
        }

        private string[] ActiveTemplatesPaths => new [] { Path.Combine(
                _webProject.ProjectDirectory,
                "Coalesce",
                "Templates")};

        private void CopyToOriginalsAndDestinationIfNeeded(string sourceFile, string originalsFile, string destinationFile)
        {
            if (FileCompare(originalsFile, destinationFile))
            {
                // The original file and the active file are the same. Overwrite the active file with the new template.
                File.Copy(sourceFile, destinationFile, true);
            }

            File.Copy(sourceFile, originalsFile, true);
        }

        private async Task CopyStaticFiles(CommandLineGeneratorModel commandLineGeneratorModel)
        {
            Console.WriteLine("Copying Static Files");
            string areaLocation = "";

            if (!string.IsNullOrWhiteSpace(commandLineGeneratorModel.AreaLocation))
            {
                areaLocation = Path.Combine("Areas", commandLineGeneratorModel.AreaLocation);
            }

            // Directory location for all "original" files from intellitect
            var baseCoalescePath = Path.Combine(
                _webProject.ProjectDirectory,
                areaLocation,
                "Coalesce");

            var originalsPath = Path.Combine( baseCoalescePath, "Originals");
            var originalTemplatesPath = Path.Combine( originalsPath, "Templates" );
            var activeTemplatesPath = Path.Combine( baseCoalescePath, "Templates" );


            Directory.CreateDirectory(baseCoalescePath);

            // We need to preserve the old intelliTect folder so that we don't overwrite any custom files,
            // since the contents of this folder are what is used to determine if changes have been made.
            // If the Coalesce folder isn't found, we will assume this is effectively a new installation of Coalesce.
            var oldOriginalsPath = Path.Combine(
                 _webProject.ProjectDirectory,
                 areaLocation,
                 "intelliTect");
            if (Directory.Exists(oldOriginalsPath))
                Directory.Move(oldOriginalsPath, originalsPath);

            Directory.CreateDirectory(originalsPath);
            Directory.CreateDirectory(originalTemplatesPath);
            Directory.CreateDirectory(activeTemplatesPath);

            // Copy over Api Folder and Files
            var apiViewOutputPath = Path.Combine(
                _webProject.ProjectDirectory,
                areaLocation,
                "Views", "Api");

            Directory.CreateDirectory(apiViewOutputPath);

            //File.Copy(TemplateLocation("Index.cshtml"), Path.Combine(apiViewOutputPath, "Index.cshtml"), true);
            //File.Copy(TemplateLocation("CreateEdit.cshtml"), Path.Combine(apiViewOutputPath, "CreateEdit.cshtml"), true);
            File.Copy(TemplateLocation("Docs.cshtml"), Path.Combine(apiViewOutputPath, "Docs.cshtml"), true);

            // Copy over Shared Folder (_EditorHtml, _Master - always, _Layout only if it doesn't exist)
            var sharedViewOutputPath = Path.Combine(
                _webProject.ProjectDirectory,
                areaLocation,
                "Views", "Shared");

            if (!Directory.Exists(sharedViewOutputPath))
            {
                Directory.CreateDirectory(sharedViewOutputPath);
            }
            File.Copy(TemplateLocation("_EditorHtml.cshtml"), Path.Combine(sharedViewOutputPath, "_EditorHtml.cshtml"), true);
            File.Copy(TemplateLocation("_Master.cshtml"), Path.Combine(sharedViewOutputPath, "_Master.cshtml"), true);

            if (string.IsNullOrWhiteSpace(commandLineGeneratorModel.AreaLocation))
            {
                CopyFileWithoutOverwritingOriginal(TemplateLocation("_Layout.cshtml"), originalsPath, sharedViewOutputPath, "_Layout.cshtml");
            }
            CopyFileWithoutOverwritingOriginal(TemplateLocation("_ViewStart.cshtml"), originalsPath, Path.Combine(_webProject.ProjectDirectory, areaLocation, "Views"), "_ViewStart.cshtml");

            // only copy the intellitect scripts when generating the root site, this isn't needed for areas since it will already exist at the root
            if (string.IsNullOrWhiteSpace(commandLineGeneratorModel.AreaLocation))
            {
                // Copy files for the scripts folder
                var scriptsOutputPath = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    "Scripts", "Coalesce");

                Directory.CreateDirectory(scriptsOutputPath);

                string[] intellitectScripts =
                {
                    "intellitect.ko.base.ts",
                    "intellitect.ko.bindings.ts",
                    "intellitect.ko.utilities.ts",
                    "intellitect.ko.utilities.ts",
                    "intellitect.utilities.ts",
                };

                foreach ( var fileName in intellitectScripts)
                {
                    File.Copy(TemplateLocation(fileName), Path.Combine(scriptsOutputPath, fileName), true);
                }



                string[] generationTemplates =
                {
                    "ApiController.cshtml",
                    "CardView.cshtml",
                    "ClassDto.cshtml",
                    "CreateEditView.cshtml",
                    "KoComplexType.cshtml",
                    "KoExternalType.cshtml",
                    "KoListViewModel.cshtml",
                    "KoViewModel.cshtml",
                    "LocalBaseApiController.cshtml",
                    "StaticDocumentationBuilder.cshtml",
                    "TableView.cshtml",
                    "ViewController.cshtml",
                };

                foreach (var fileName in generationTemplates)
                {
                    string embeddedFile = TemplateLocation( fileName );
                    string originalFile = Path.Combine( originalTemplatesPath, fileName );
                    string activeFile = Path.Combine( activeTemplatesPath, fileName );

                    if (FileCompare(originalFile, activeFile))
                    {
                        // The original file and the active file are the same. Overwrite the active file with the new template.
                        File.Copy(embeddedFile, activeFile, true);
                    }

                    File.Copy(embeddedFile, originalFile, true);
                }
            }

            var stylesOutputPath = Path.Combine(
                _webProject.ProjectDirectory,
                areaLocation,
                "Styles");

            if (!Directory.Exists(stylesOutputPath))
            {
                Directory.CreateDirectory(stylesOutputPath);
            }
            CopyFileWithoutOverwritingOriginal(TemplateLocation("site.scss"), originalsPath, stylesOutputPath, "site.scss");

            // if generating an area, assume the root site already has all "plumbing" needed
            if (string.IsNullOrWhiteSpace(commandLineGeneratorModel.AreaLocation))
            {
                // Copy files from ProjectConfig if they don't already exist
                CopyFileWithoutOverwritingOriginal(TemplateLocation("bower.json"), originalsPath, _webProject.ProjectDirectory, "bower.json");
                CopyFileWithoutOverwritingOriginal(TemplateLocation(".bowerrc"), originalsPath, _webProject.ProjectDirectory, ".bowerrc");
                CopyFileWithoutOverwritingOriginal(TemplateLocation("gulpfile.js"), originalsPath, _webProject.ProjectDirectory, "gulpfile.js");
                CopyFileWithoutOverwritingOriginal(TemplateLocation("package.json"), originalsPath, _webProject.ProjectDirectory, "package.json");
                CopyFileWithoutOverwritingOriginal(TemplateLocation("tsconfig.json"), originalsPath, _webProject.ProjectDirectory, "tsconfig.json");
                CopyFileWithoutOverwritingOriginal(TemplateLocation("tsd.json"), originalsPath, _webProject.ProjectDirectory, "tsd.json");

                if (!commandLineGeneratorModel.OnlyGenerateFiles)
                {
                    if (File.Exists(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\npm.cmd"))
                    {
                        Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\npm.cmd", "install").WaitForExit();
                    }
                    if (File.Exists(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\bower.cmd"))
                    {
                        Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\bower.cmd", "install").WaitForExit();
                    }

                    // TODO
                    //if (!File.Exists(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\node\tsd.cmd"))
                    //{
                    //    Console.WriteLine("Installing TSD");
                    //    // install it
                    //    ProcessStartInfo info = new ProcessStartInfo(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\npm.cmd");
                    //    info.UseShellExecute = true;
                    //    info.Verb = "runas";
                    //    info.Arguments = "install tsd -g";
                    //    Process.Start(info).WaitForExit();
                    //}
                    // only run the init command if the tsd.json file does not already exist. We most likely will never have this run since we copy our own version earlier, but putting it here for completeness
                    //Console.WriteLine("Installing TypeScript Definitions");
                    //if (!File.Exists(Path.Combine(_webProject.ProjectDirectory, "tsd.json")))
                    //{
                    //    Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\node\tsd.cmd", "init").WaitForExit();
                    //}
                    //Process.Start(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\Extensions\Microsoft\Web Tools\External\node\tsd.cmd", "reinstall").WaitForExit();
                }
                // Copy readme to root as IntelliTectScaffoldingReadme.txt
                File.Copy(TemplateLocation("Readme.txt"), Path.Combine(_webProject.ProjectDirectory, "IntelliTectScaffoldingReadme.txt"), true);
            }
            await CodeGeneratorActionsService.AddFileFromTemplateAsync(Path.Combine(_webProject.ProjectDirectory, areaLocation, "Views", "_ViewImports.cshtml"),
                "_ViewImports.cshtml", TemplateFolders, null);
            await CodeGeneratorActionsService.AddFileFromTemplateAsync(Path.Combine(sharedViewOutputPath, "_AdminLayout.cshtml"), "_AdminLayout.cshtml", TemplateFolders, commandLineGeneratorModel.AreaLocation);
        }

        private string TemplateLocation(string templateName = null)
        {
            FilesLocator locator = new FilesLocator();
            string filePath = locator.GetFilePath(templateName, TemplateFolders);

            return filePath;
        }

        private void CopyFileWithoutOverwritingOriginal(string inputPath, string intelliTectTemplatePath, string outputPath, string fileToCopy)
        {
            if (!Directory.Exists(intelliTectTemplatePath))
            {
                Directory.CreateDirectory(intelliTectTemplatePath);
            }
            // if the file in the source matches the current IntelliTect original file
            if (FileCompare(Path.Combine(intelliTectTemplatePath, fileToCopy), Path.Combine(outputPath, fileToCopy)))
            {
                // override the file
                //Console.WriteLine($"Writing {fileToCopy}");
                File.Copy(inputPath, Path.Combine(outputPath, fileToCopy), true);
            }

            File.Copy(inputPath, Path.Combine(intelliTectTemplatePath, fileToCopy), true);
        }

        private bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            if (!File.Exists(file1))
            {
                return true;
            }

            if (!File.Exists(file2))
            {
                return true;
            }

            // Open the two files.
            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            // Check the file sizes. If they are not the same, the files 
            // are not the same.
            if (fs1.Length != fs2.Length)
            {
                // Close the file
                fs1.Close();
                fs2.Close();

                // Return false to indicate files are different
                return false;
            }

            // Read and compare a byte from each file until either a
            // non-matching set of bytes is found or until the end of
            // file1 is reached.
            do
            {
                // Read one byte from each file.
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            // Close the files.
            fs1.Close();
            fs2.Close();

            // Return the success of the comparison. "file1byte" is 
            // equal to "file2byte" at this point only if the files are 
            // the same.
            return ((file1byte - file2byte) == 0);
        }

        private async Task GenerateScripts(
            CommandLineGeneratorModel controllerGeneratorModel,
            List<ClassViewModel> models,
            ModelType dataContext,
            string targetNamespace)
        {
            string areaLocation = "";

            if (!string.IsNullOrWhiteSpace(controllerGeneratorModel.AreaLocation))
            {
                areaLocation = Path.Combine("Areas", controllerGeneratorModel.AreaLocation);
            }

            // TODO: do we need this anymore?
            //var layoutDependencyInstaller = ActivatorUtilities.CreateInstance<MvcLayoutDependencyInstaller>(ServiceProvider);
            //await layoutDependencyInstaller.Execute();

            ViewModelForTemplates apiModels = new ViewModelForTemplates
            {
                Models = models,
                ContextInfo = dataContext,
                Namespace = targetNamespace,
                AreaName = controllerGeneratorModel.AreaLocation,
                ModulePrefix = controllerGeneratorModel.TypescriptModulePrefix
            };

            // Copy over the static files
            await CopyStaticFiles(controllerGeneratorModel);

            var apiViewOutputPath = Path.Combine(
                _webProject.ProjectDirectory,
                areaLocation,
                "Views", "Api");

            if (!Directory.Exists(apiViewOutputPath))
            {
                Directory.CreateDirectory(apiViewOutputPath);
            }

            Console.WriteLine("Generating Code");
            Console.WriteLine("-- Generating DTOs");
            Console.Write("   ");
            foreach (var model in apiModels.ViewModelsForTemplates)
            {
                Console.Write($"{model.Model.Name}  ");

                var filename = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    "Models", "Generated",
                    model.Model.Name + "DtoGen.cs");

                await CodeGeneratorActionsService.AddFileFromTemplateAsync(filename, "ClassDto.cshtml", ActiveTemplatesPaths, model);
            }
            Console.WriteLine();


            Console.WriteLine("-- Generating Models");
            Console.Write("   ");
            foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.OnContext))
            {
                Console.Write($"{model.Model.Name}  ");
                var fileName = (string.IsNullOrWhiteSpace(model.ModulePrefix)) ? $"Ko.{model.Model.Name}.ts" : $"Ko.{model.ModulePrefix}.{model.Model.Name}.ts";
                // Todo: Need logic for areas
                var viewOutputPath = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    ScriptsFolderName, "Generated",
                    fileName);

                await CodeGeneratorActionsService.AddFileFromTemplateAsync(viewOutputPath,
                    "KoViewModel.cshtml", ActiveTemplatesPaths, model);

                //Console.WriteLine("   Added Script: " + viewOutputPath);

                fileName = (string.IsNullOrWhiteSpace(model.ModulePrefix)) ? $"Ko.{model.Model.ListViewModelClassName}.ts" : $"Ko.{model.ModulePrefix}.{model.Model.ListViewModelClassName}.ts";
                viewOutputPath = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    ScriptsFolderName, "Generated",
                    fileName);

                await CodeGeneratorActionsService.AddFileFromTemplateAsync(viewOutputPath,
                    "KoListViewModel.cshtml", ActiveTemplatesPaths, model);

                //Console.WriteLine("   Added Script: " + viewOutputPath);
            }
            Console.WriteLine();


            Console.WriteLine("-- Generating External Types");
            Console.Write("   ");
            foreach (var externalType in apiModels.ViewModelsForTemplates.Where(f => !f.Model.OnContext))
            {
                var fileName = (string.IsNullOrWhiteSpace(externalType.ModulePrefix)) ? $"Ko.{externalType.Model.Name}.ts" : $"Ko.{externalType.ModulePrefix}.{externalType.Model.Name}.ts";
                // Todo: Need logic for areas
                var viewOutputPath = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    ScriptsFolderName, "Generated",
                    fileName);

                await CodeGeneratorActionsService.AddFileFromTemplateAsync(viewOutputPath,
                    "KoExternalType.cshtml", ActiveTemplatesPaths, externalType);

                Console.Write(externalType.Model.Name + "  ");
            }
            Console.WriteLine();

            Console.WriteLine("-- Generating Complex Types");
            ViewModelForTemplates complexTypes = new ViewModelForTemplates
            {
                Models = ComplexTypes(models),
                ContextInfo = dataContext,
                Namespace = targetNamespace,
                AreaName = controllerGeneratorModel.AreaLocation
            };
            foreach (var complexType in complexTypes.ViewModelsForTemplates)
            {
                var fileName = (string.IsNullOrWhiteSpace(complexType.ModulePrefix)) ? $"Ko.{complexType.Model.Name}.ts" : $"Ko.{complexType.ModulePrefix}.{complexType.Model.Name}.ts";
                // Todo: Need logic for areas
                var viewOutputPath = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    ScriptsFolderName, "Generated",
                    fileName);

                await CodeGeneratorActionsService.AddFileFromTemplateAsync(viewOutputPath,
                    "KoComplexType.cshtml", ActiveTemplatesPaths, complexType);

                Console.WriteLine("   Added: " + viewOutputPath);
            }


            Console.WriteLine("-- Generating API Controllers");
            // Generate base api controller if it doesn't already exist
            {
                var filename = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    "Api", "Generated",
                    "LocalBaseApiController.cs");

                var model = apiModels.ViewModelsForTemplates.First(f => f.Model.OnContext);

                await CodeGeneratorActionsService.AddFileFromTemplateAsync(filename, "LocalBaseApiController.cshtml", ActiveTemplatesPaths, model);
            }

            // Generate model api controllers
            foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.OnContext))
            {
                var filename = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    "Api", "Generated",
                    model.Model.Name + "ApiControllerGen.cs");
                await CodeGeneratorActionsService.AddFileFromTemplateAsync(filename,
                    "ApiController.cshtml", ActiveTemplatesPaths, model);
            }


            Console.WriteLine("-- Generating View Controllers");

            foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.OnContext))
            {
                var filename = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    "Controllers", "Generated",
                    model.Model.Name + "ControllerGen.cs");
                await CodeGeneratorActionsService.AddFileFromTemplateAsync(filename,
                    "ViewController.cshtml", ActiveTemplatesPaths, model);
            }

            Console.WriteLine("-- Generating Views");
            foreach (var model in apiModels.ViewModelsForTemplates.Where(f => f.Model.OnContext))
            {
                var filename = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    "Views", "Generated",
                    model.Model.Name,
                    "Table.cshtml");
                await CodeGeneratorActionsService.AddFileFromTemplateAsync(filename,
                    "TableView.cshtml", ActiveTemplatesPaths, model);

                filename = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    "Views", "Generated",
                    model.Model.Name,
                    "Cards.cshtml");
                await CodeGeneratorActionsService.AddFileFromTemplateAsync(filename,
                    "CardView.cshtml", ActiveTemplatesPaths, model);

                filename = Path.Combine(
                    _webProject.ProjectDirectory,
                    areaLocation,
                    "Views", "Generated",
                    model.Model.Name,
                    "CreateEdit.cshtml");
                await CodeGeneratorActionsService.AddFileFromTemplateAsync(filename,
                    "CreateEditView.cshtml", ActiveTemplatesPaths, model);
            }


            //await layoutDependencyInstaller.InstallDependencies();

            var scriptOutputPath = Path.Combine(_webProject.ProjectDirectory, ScriptsFolderName);
            GenerateTSReferenceFile(scriptOutputPath);

            //await GenerateTypeScriptDocs(scriptOutputPath);

            Console.WriteLine("-- Generation Complete --");
        }

        private async Task GenerateTypeScriptDocs(string path)
        {
            var dir = new DirectoryInfo(path);
            Console.WriteLine($"-- Creating Doc Files");
            foreach (var file in dir.GetFiles("*.ts"))
            {
                // don't gen json documentation for definition files
                if (!file.FullName.EndsWith(".d.ts"))
                {
                    var reader = file.OpenText();
                    var doc = new TypeScriptDocumentation();
                    doc.TsFilename = file.Name;
                    doc.Generate(await reader.ReadToEndAsync());
                    var serializer = Newtonsoft.Json.JsonSerializer.Create();
                    // Create the doc file.
                    FileInfo docFile = new FileInfo(file.FullName.Replace(".ts", ".json"));
                    // Remove it if it exists.
                    try
                    {
                        if (docFile.Exists) docFile.Delete();
                    }
                    catch (Exception)
                    {
                        System.Threading.Thread.Sleep(3000);
                        try
                        {
                            if (docFile.Exists) docFile.Delete();
                        }
                        catch (Exception)
                        {
                            Console.WriteLine($"Could not delete file {docFile.FullName}");
                        }
                    }
                    using (var tw = docFile.CreateText())
                    {
                        serializer.Serialize(tw, doc);
                        tw.Close();
                    }
                }
            }
        }

        private void GenerateTSReferenceFile(string path)
        {
            List<string> fileContents = new List<string>();
            var dir = new DirectoryInfo(path + "\\Coalesce");

            foreach (var file in dir.GetFiles("*.ts"))
            {
                if ((file.Name.StartsWith("intellitect", true, CultureInfo.InvariantCulture) || file.Name.StartsWith("ko.", true, CultureInfo.InvariantCulture)) &&
                    !file.Name.EndsWith(".d.ts"))
                {
                    fileContents.Add($"/// <reference path=\"\\Coalesce\\{file.Name}\" />");
                }
            }

            // Do files in the Generated folder.
            dir = new DirectoryInfo(path + "\\Generated");
            foreach (var file in dir.GetFiles("*.ts"))
            {
                if ((file.Name.StartsWith("intellitect", true, CultureInfo.InvariantCulture) || file.Name.StartsWith("ko.", true, CultureInfo.InvariantCulture)) &&
                    !file.Name.EndsWith(".d.ts"))
                {
                    fileContents.Add($"/// <reference path=\"Generated\\{file.Name}\" />");
                }
            }

            // Write the file with the array list of content.
            File.WriteAllLines(Path.Combine(path, "intellitect.references.d.ts"), fileContents);
        }

        public IEnumerable<string> TemplateFolders
        {
            get
            {
                return Common.TemplateFoldersUtilities.GetTemplateFolders(
                    codeGenAssembly: ThisAssemblyName,
                    mvcProject: _webProject,
                    baseFolders: new[] { "" }
                );
            }
        }

        /// <summary>
        /// Gets a list of all the complex types used in the models.
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public IEnumerable<ClassViewModel> ComplexTypes(List<ClassViewModel> models)
        {
            Dictionary<string, ClassViewModel> complexTypes = new Dictionary<string, ClassViewModel>();

            foreach (var model in models)
            {
                foreach (var prop in model.Properties.Where(f => f.IsComplexType))
                {
                    if (!complexTypes.ContainsKey(prop.Name))
                    {
                        var ctModel = ReflectionRepository.GetClassViewModel(prop.Type);
                        complexTypes.Add(prop.Name, ctModel);
                    }
                }
            }

            return complexTypes.Values;
        }

    }
}
