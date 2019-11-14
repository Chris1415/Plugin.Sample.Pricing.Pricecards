using Plugin.Sample.Pricing.Pricecards.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Pricing;
using System;
using System.Threading.Tasks;

namespace Plugin.Sample.Pricing.Pricecards
{
    public class CustomAddPriceSnapshotCommand : AddPriceSnapshotCommand
    {
        private readonly IAddPriceSnapshotPipeline _addPriceSnapshotPipeline;

        public CustomAddPriceSnapshotCommand(IAddPriceSnapshotPipeline addPriceSnapshotPipeline, IFindEntityPipeline findEntityPipeline, IServiceProvider serviceProvider)
          : base(addPriceSnapshotPipeline, findEntityPipeline, serviceProvider)
        {
            this._addPriceSnapshotPipeline = addPriceSnapshotPipeline;
        }

        public virtual async Task<PriceCard> Process(CommerceContext commerceContext, string cardFriendlyId, DateTimeOffset beginDate, DateTimeOffset endDate)
        {
            AddPriceSnapshotCommand priceSnapshotCommand = this;
            PriceCard result = null;
            using (CommandActivity.Start(commerceContext, priceSnapshotCommand))
            {
                PriceCard priceCard = await this.GetPriceCard(commerceContext, cardFriendlyId).ConfigureAwait(false);
                if (priceCard == null)
                {
                    return null;
                }

                var pricingSnapshot = new PriceSnapshotComponent(beginDate);
                var endDateComponent = pricingSnapshot.GetComponent<SnapshotEndDateComponent>();
                endDateComponent.EndDate = endDate;

                await priceSnapshotCommand.PerformTransaction(commerceContext, (async () => result = await this._addPriceSnapshotPipeline.Run(new PriceCardSnapshotArgument(priceCard, pricingSnapshot), commerceContext.PipelineContextOptions).ConfigureAwait(false))).ConfigureAwait(false);
                return result;
            }
        }
    }
}
