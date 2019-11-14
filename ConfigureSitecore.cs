// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Pricing.Pricecards
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.EntityViews;
    using Sitecore.Commerce.Plugin.Catalog;
    using Sitecore.Commerce.Plugin.Pricing;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config

             .ConfigurePipeline<IGetEntityViewPipeline>(d => d
                .Replace<GetPriceSnapshotDetailsViewBlock, CustomGetPriceSnapshotDetailsViewBlock>())
             .ConfigurePipeline<IDoActionPipeline>(d => d
                .Replace<DoActionAddPriceSnapshotBlock, CustomDoActionAddPriceSnapshotBlock>())
             .ConfigurePipeline<ICalculateSellableItemSellPricePipeline>(d => d
                .Replace<CalculateSellableItemSellPriceBlock, CustomCalculateSellableItemSellPriceBlock>())
             .ConfigurePipeline<ICalculateVariationsSellPricePipeline>(d => d
                .Replace<CalculateVariationsSellPriceBlock, CustomCalculateVariationsSellPriceBlock>())
             .ConfigurePipeline<IConfigureServiceApiPipeline>(configure => configure.Add<ConfigureServiceApiBlock>()));

            services.RegisterAllCommands(assembly);
        }
    }
}