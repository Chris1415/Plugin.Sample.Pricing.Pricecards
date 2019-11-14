using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Pipelines;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.Sample.Pricing.Pricecards
{
    [PipelineDisplayName("Pricing.block.customdoactionaddpricesnapshot")]
    public class CustomDoActionAddPriceSnapshotBlock : PipelineBlock<EntityView, EntityView, CommercePipelineExecutionContext>
    {
        private readonly CustomAddPriceSnapshotCommand _addPriceSnapshotCommand;

        public CustomDoActionAddPriceSnapshotBlock(CustomAddPriceSnapshotCommand addPriceSnapshotCommand)
        {
            this._addPriceSnapshotCommand = addPriceSnapshotCommand;
        }

        public override async Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            if (string.IsNullOrEmpty(arg?.Action) 
                || !arg.Action.Equals(context.GetPolicy<KnownPricingActionsPolicy>().AddPriceSnapshot, StringComparison.OrdinalIgnoreCase) 
                || context.CommerceContext.GetObjects<PriceCard>().FirstOrDefault(p => p.Id.Equals(arg.EntityId, StringComparison.OrdinalIgnoreCase)) == null)
            {
                return arg;
            }

            ViewProperty beginDateViewProperty = arg.Properties.FirstOrDefault((p => p.Name.Equals("BeginDate", StringComparison.OrdinalIgnoreCase)));
            if (!DateTimeOffset.TryParse(beginDateViewProperty?.Value, null, DateTimeStyles.AdjustToUniversal, out DateTimeOffset resultBeginDate))
            {
                string str1 = beginDateViewProperty == null ? "BeginDate" : beginDateViewProperty.DisplayName;
                string str2 = await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().ValidationError, "InvalidOrMissingPropertyValue", new object[1]
                {
                    str1
                }, "Invalid or missing value for property 'BeginDate'.").ConfigureAwait(false);
                return arg;
            }

            ViewProperty endDateViewProperty = arg.Properties.FirstOrDefault(p => p.Name.Equals("EndDate", StringComparison.OrdinalIgnoreCase));
            if (!DateTimeOffset.TryParse(endDateViewProperty?.Value, null, DateTimeStyles.AdjustToUniversal, out DateTimeOffset resultEndDate))
            {
                string str1 = endDateViewProperty == null ? "EndDate" : endDateViewProperty.DisplayName;
                string str2 = await context.CommerceContext.AddMessage(context.GetPolicy<KnownResultCodes>().ValidationError, "InvalidOrMissingPropertyValue", new object[1]
                {
                    str1
                }, "Invalid or missing value for property 'BeginDate'.").ConfigureAwait(false);
                return arg;
            }

            ViewProperty useEndDateViewProerty = arg.Properties.FirstOrDefault((p => p.Name.Equals("UseEndDate", StringComparison.OrdinalIgnoreCase)));

            PriceCard priceCard = await this._addPriceSnapshotCommand.Process(context.CommerceContext, arg.EntityId, resultBeginDate, useEndDateViewProerty.Value.Equals("true") ? resultEndDate : DateTimeOffset.MinValue).ConfigureAwait(false);
            return arg;
        }
    }
}
