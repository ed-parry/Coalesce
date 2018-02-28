﻿using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Vue.Generators
{
    public class Scripts : CompositeGenerator<ReflectionRepository>
    {
        public Scripts(CompositeGeneratorServices services) : base(services) { }

        //public override IEnumerable<ICleaner> GetCleaners()
        //{
        //    yield return Cleaner<DirectoryCleaner>()
        //        .AppendTargetPath("Generated");
        //}


        public override IEnumerable<IGenerator> GetGenerators()
        {
            yield return Generator<TsMetadata>()
                .WithModel(Model)
                .AppendOutputPath("metadata.g.ts");

            yield return Generator<TsModels>()
                .WithModel(Model)
                .AppendOutputPath("models.g.ts");

            //var dynamicGenerators = GetDynamicGenerators().ToList();
            //foreach (var gen in dynamicGenerators)
            //{
            //    yield return gen;
            //}

            //yield return Generator<TsReferencesFile>()
            //    .WithModel(dynamicGenerators)
            //    .AppendOutputPath("viewmodels.generated.d.ts");
        }


        private IEnumerable<IGenerator> GetDynamicGenerators()
        {
            string Partial(ClassViewModel model) => model.HasTypeScriptPartial ? ".Partial" : "";

            foreach (var model in Model.ApiBackedClasses)
            {
                //yield return Generator<KoViewModel>()
                //    .WithModel(model)
                //    .AppendOutputPath($"Generated/Ko.{model.Name}{Partial(model)}.g.ts");
                //yield return Generator<KoListViewModel>()
                //    .WithModel(model)
                //    .AppendOutputPath($"Generated/Ko.{model.ListViewModelClassName}.g.ts");
            }

            foreach (var model in Model.ExternalTypes)
            {
                //yield return Generator<KoExternalType>()
                //    .WithModel(model)
                //    .AppendOutputPath($"Generated/Ko.{model.Name}{Partial(model)}.g.ts");
            }

            foreach (var model in Model.DiscoveredClassViewModels.Where(c => c.HasTypeScriptPartial))
            {
                //yield return Generator<KoTsPartialStub>()
                //    .WithModel(model)
                //    .AppendOutputPath($"Partials/Ko.{model.Name}.partial.ts");
            }

            yield break;
        }
    }
}