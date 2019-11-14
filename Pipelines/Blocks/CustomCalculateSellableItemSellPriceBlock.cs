using Plugin.Sample.Pricing.Pricecards.Components;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plugin.Sample.Pricing.Pricecards
{
    [PipelineDisplayName("Catalog.block.customcalculatesellableitemsellprice")]
    public class CustomCalculateSellableItemSellPriceBlock : CalculateSellableItemSellPriceBlock
    {
        public CustomCalculateSellableItemSellPriceBlock(CommerceCommander commander)
          : base(commander)
        {
        }

        protected override PriceSnapshotComponent FilterPriceSnapshotsByDate(PriceCard priceCard, CommercePipelineExecutionContext context)
        {
            if (priceCard == null || context == null)
            {
                return null;
            }

            DateTimeOffset effectiveDate = context.CommerceContext.CurrentEffectiveDate();
            PriceSnapshotComponent snapshotComponent1 = priceCard.Snapshots.Where(s =>
            {
                var snapshotEndDateComponent = s.GetComponent<SnapshotEndDateComponent>();
                if (s.IsApproved(context.CommerceContext))
                {
                    bool startDateReached = s.BeginDate.CompareTo(effectiveDate) <= 0;
                    bool endDateNotReached = snapshotEndDateComponent.EndDate != DateTimeOffset.MinValue
                        ? startDateReached && snapshotEndDateComponent.EndDate.CompareTo(effectiveDate) >= 0
                        : true;
                    return startDateReached && endDateNotReached;
                }
                return false;
            }).OrderByDescending(s => s.BeginDate).FirstOrDefault();
            if (snapshotComponent1 == null)
            {
                return null;
            }

            PriceSnapshotComponent snapshotComponent2 = new PriceSnapshotComponent(snapshotComponent1.BeginDate)
            {
                Id = snapshotComponent1.Id,
                ChildComponents = snapshotComponent1.ChildComponents,
                SnapshotComponents = snapshotComponent1.SnapshotComponents,
                Tags = snapshotComponent1.Tags
            };
            string currency = context.CommerceContext.CurrentCurrency();
            snapshotComponent2.Tiers = snapshotComponent1
                .Tiers
                .Where(t => t.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return snapshotComponent2;
        }

        protected override PriceSnapshotComponent FilterPriceSnapshotsByTags(IList<PriceCard> cards, IList<Tag> tags, CommercePipelineExecutionContext context)
        {
            if (cards == null || !cards.Any() || tags == null || !tags.Any() || context == null)
            {
                return null;
            }

            DateTimeOffset effectiveDate = context.CommerceContext.CurrentEffectiveDate();
            List<PriceSnapshotComponent> allSnapshots = new List<PriceSnapshotComponent>();
            cards.ForEach(card => allSnapshots.AddRange(card.Snapshots.Where(s => s.Tags.Any())));
            PriceSnapshotComponent snapshotComponent1 = allSnapshots.Where(s =>
            {
                var snapshotEndDateComponent = s.GetComponent<SnapshotEndDateComponent>();
                if (s.IsApproved(context.CommerceContext))
                {
                    bool startDateReached = s.BeginDate.CompareTo(effectiveDate) <= 0;
                    bool endDateNotReached = snapshotEndDateComponent.EndDate != DateTimeOffset.MinValue
                        ? startDateReached && snapshotEndDateComponent.EndDate.CompareTo(effectiveDate) >= 0
                        : true;
                    return startDateReached && endDateNotReached;
                }
                return false;
            })
            .Where(s => tags
                .Select(t => t.Name)
                .Intersect(s.Tags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase)
                .Any())
            .OrderByDescending(s => s.Tags
                .Select(t => t.Name)
                .Intersect(tags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase)
                .Count())
            .ThenByDescending(s => s.BeginDate).FirstOrDefault();
            if (snapshotComponent1 == null)
            {
                return null;
            }

            PriceSnapshotComponent snapshotComponent2 = new PriceSnapshotComponent(snapshotComponent1.BeginDate)
            {
                Id = snapshotComponent1.Id,
                ChildComponents = snapshotComponent1.ChildComponents,
                SnapshotComponents = snapshotComponent1.SnapshotComponents,
                Tags = snapshotComponent1.Tags
            };

            string currency = context.CommerceContext.CurrentCurrency();
            snapshotComponent2.Tiers = snapshotComponent1.Tiers
                .Where(t => t.Currency.Equals(currency, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return snapshotComponent2;
        }
    }
}
