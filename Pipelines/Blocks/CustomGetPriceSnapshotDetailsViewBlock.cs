using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Sitecore.Commerce.Plugin.Pricing;
using Plugin.Sample.Pricing.Pricecards.Components;

namespace Plugin.Sample.Pricing.Pricecards
{
    [PipelineDisplayName("Pricing.block.customgetpricesnapshotdetailsView")]
    public class CustomGetPriceSnapshotDetailsViewBlock : PricingViewBlock
    {
        public override Task<EntityView> Run(EntityView arg, CommercePipelineExecutionContext context)
        {
            Condition.Requires(arg).IsNotNull(this.Name + ": The argument cannot be null");
            EntityViewArgument request = context.CommerceContext.GetObjects<EntityViewArgument>().FirstOrDefault();
            if (request == null)
            {
                return Task.FromResult(arg);
            }

            bool isAddAction = request.ForAction.Equals(context.GetPolicy<KnownPricingActionsPolicy>().AddPriceSnapshot, StringComparison.OrdinalIgnoreCase);
            bool isEditAction = request.ForAction.Equals(context.GetPolicy<KnownPricingActionsPolicy>().EditPriceSnapshot, StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrEmpty(request?.ViewName)
                || !(request.Entity is PriceCard)
                || !request.ViewName.Equals(context.GetPolicy<KnownPricingViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase)
                && !request.ViewName.Equals(context.GetPolicy<KnownPricingViewsPolicy>().PriceCardSnapshots, StringComparison.OrdinalIgnoreCase)
                && !request.ViewName.Equals(context.GetPolicy<KnownPricingViewsPolicy>().PriceSnapshotDetails, StringComparison.OrdinalIgnoreCase)
                || request.ViewName.Equals(context.GetPolicy<KnownPricingViewsPolicy>().PriceSnapshotDetails, StringComparison.OrdinalIgnoreCase)
                && string.IsNullOrEmpty(request.ItemId) && !isAddAction)
            {
                return Task.FromResult(arg);
            }

            PriceCard card = (PriceCard)request.Entity;
            if (request.ViewName.Equals(context.GetPolicy<KnownPricingViewsPolicy>().Master, StringComparison.OrdinalIgnoreCase)
                || request.ViewName.Equals(context.GetPolicy<KnownPricingViewsPolicy>().PriceCardSnapshots, StringComparison.OrdinalIgnoreCase))
            {
                List<EntityView> views = new List<EntityView>();
                this.FindViews(views, arg, context.GetPolicy<KnownPricingViewsPolicy>().PriceSnapshotDetails, context.CommerceContext);
                views.ForEach(snapshotDetailsView =>
                {
                    EntityView view = snapshotDetailsView;
                    PriceCard priceCard = card;
                    PriceSnapshotComponent snapshot = priceCard?.Snapshots.FirstOrDefault(s => s.Id.Equals(snapshotDetailsView.ItemId, StringComparison.OrdinalIgnoreCase));
                    int num1 = isAddAction ? 1 : 0;
                    int num2 = isEditAction ? 1 : 0;
                    this.PopulateSnapshotDetails(view, snapshot, num1 != 0, num2 != 0);
                });
                return Task.FromResult(arg);
            }
            PriceSnapshotComponent snapshotComponent;
            if (isAddAction)
            {
                snapshotComponent = null;
            }
            else
            {
                PriceCard priceCard = card;
                snapshotComponent = priceCard?.Snapshots.FirstOrDefault(s => s.Id.Equals(request.ItemId, StringComparison.OrdinalIgnoreCase));
            }
            PriceSnapshotComponent snapshot1 = snapshotComponent;
            this.PopulateSnapshotDetails(arg, snapshot1, isAddAction, isEditAction);
            var rawValue = arg.Properties.FirstOrDefault((p => p.Name.Equals("BeginDate", StringComparison.OrdinalIgnoreCase))).RawValue;
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(context.CommerceContext.CurrentLanguage());
            string str = (string.IsNullOrEmpty(card.DisplayName) ? card.Name : card.DisplayName) + " (" + rawValue + ")";
            ViewProperty viewProperty1 = arg.Properties.FirstOrDefault(p => p.Name.Equals("DisplayName", StringComparison.OrdinalIgnoreCase));
            if (viewProperty1 != null)
            {
                viewProperty1.RawValue = str;
            }

            else if (!isAddAction && !isEditAction)
            {
                List<ViewProperty> properties = arg.Properties;
                ViewProperty viewProperty2 = new ViewProperty
                {
                    Name = "DisplayName",
                    RawValue = str,
                    IsReadOnly = true
                };
                properties.Add(viewProperty2);
            }
            return Task.FromResult(arg);
        }

        protected virtual void PopulateSnapshotDetails(EntityView view, PriceSnapshotComponent snapshot, bool isAddAction, bool isEditAction)
        {
            if (view == null)
            {
                return;
            }

            var snapshotEndDateComponent = snapshot?.GetComponent<SnapshotEndDateComponent>();
            List<ViewProperty> properties1 = view.Properties;
            ViewProperty viewPropertyEndDate = null;
            ViewProperty viewPropertyStartDate = null;
            ViewProperty viewProertyUseEndDate = null;
            bool isReadOnly = !isAddAction && !isEditAction;
            if (isReadOnly)
            {
                viewPropertyStartDate = new ViewProperty
                {
                    Name = "BeginDate",
                    RawValue = snapshot != null ? snapshot.BeginDate.ToString("g") : DateTimeOffset.UtcNow.ToString("g"),
                    IsReadOnly = true,
                    UiType = "ItemLink"
                };
                viewPropertyEndDate = new ViewProperty
                {
                    Name = "EndDate",
                    RawValue = snapshot != null && snapshotEndDateComponent != null && snapshotEndDateComponent.EndDate != DateTimeOffset.MinValue
                        ? snapshotEndDateComponent.EndDate.ToString("g")
                        : string.Empty,
                    IsReadOnly = true,
                    UiType = "ItemLink"
                };
            }
            else
            {
                viewPropertyStartDate = new ViewProperty
                {
                    Name = "BeginDate",
                    RawValue = snapshot != null ? snapshot.BeginDate : DateTimeOffset.UtcNow,
                    IsReadOnly = false,
                    UiType = "ItemLink"
                };
                viewPropertyEndDate = new ViewProperty
                {
                    Name = "EndDate",
                    RawValue = snapshot != null && snapshotEndDateComponent != null
                        ? snapshotEndDateComponent.EndDate
                        : DateTimeOffset.UtcNow,
                    IsReadOnly = false,
                    UiType = "ItemLink"
                };
                viewProertyUseEndDate = new ViewProperty
                {
                    Name = "UseEndDate",
                    RawValue = true,
                    IsReadOnly = false,
                    UiType = "Checkbox"
                };
            }

            if (viewPropertyStartDate != null)
            {
                properties1.Add(viewPropertyStartDate);
            }
            if (viewPropertyEndDate != null)
            {
                properties1.Add(viewPropertyEndDate);
            }
            if (viewProertyUseEndDate != null)
            {
                properties1.Add(viewProertyUseEndDate);
            }


            if (!isAddAction)
            {
                List<string> stringList = new List<string>();
                if (snapshot?.Tags != null && snapshot.Tags.Any())
                {
                    stringList = snapshot?.Tags.Where((t => !t.Excluded)).Select((t => t.Name)).ToList();
                }

                List<ViewProperty> properties2 = view.Properties;
                ViewProperty viewProperty2 = new ViewProperty(new List<Policy>())
                {
                    Name = "IncludedTags",
                    RawValue = stringList.ToArray(),
                    IsReadOnly = !isEditAction,
                    IsRequired = false,
                    UiType = isEditAction ? "Tags" : "List",
                    OriginalType = "List"
                };
                properties2.Add(viewProperty2);
            }
            if (isAddAction | isEditAction || snapshot == null)
            {
                return;
            }

            List<ViewProperty> properties3 = view.Properties;
            ViewProperty viewProperty3 = new ViewProperty
            {
                Name = "ItemId",
                RawValue = snapshot.Id,
                IsReadOnly = true,
                IsHidden = true
            };
            properties3.Add(viewProperty3);
            List<ViewProperty> properties4 = view.Properties;
            ViewProperty viewProperty4 = new ViewProperty
            {
                Name = "Status",
                RawValue = snapshot.GetComponent<ApprovalComponent>().Status,
                IsReadOnly = true
            };
            properties4.Add(viewProperty4);
        }
    }
}
