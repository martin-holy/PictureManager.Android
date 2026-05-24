using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using MH.UI;
using MH.UI.Android.Controls;
using MH.UI.Android.Controls.Items;
using MH.UI.Android.Extensions;
using MH.UI.Android.Utils;
using MH.UI.Android.Views;
using MH.Utils;
using MH.Utils.Interfaces;
using PictureManager.Common.Features.Viewer;

namespace PictureManager.Android.Views.Entities;

public sealed class ViewerV : ScrollView {
  private readonly SelectableItemsView<IListItem> _includedFolders;
  private readonly SelectableItemsView<IListItem> _excludedFolders;
  private readonly SelectableItemsView<IListItem> _categoryGroups;
  private readonly SelectableItemsView<IListItem> _excludedKeywords;

  public ViewerV(Context context, ViewerVM dataContext) : base(context) {
    var container = LayoutU.Vertical(context);

    _includedFolders = new(context, [], x => new ListItemV(x));
    _excludedFolders = new(context, [], x => new ListItemV(x));
    // TODO 
    _categoryGroups = new(context, dataContext.CategoryGroups, x => new ListItemV(x));
    _categoryGroups.Selection.IsMultiSelect = true;
    _categoryGroups.SetLayoutManager(new LinearLayoutManager(context, LinearLayoutManager.Vertical, false));
    //_categoryGroups.ItemClickedEvent += item => // set IsSelected and call VIewerVM._updateExcludedCategoryGroups
    _excludedKeywords = new(context, [], x => new ListItemV(x));

    container.AddView(_createIncludedFolders(), LPU.LinearMatchWrap());
    container.AddView(_createExcludedFolders(), LPU.LinearMatchWrap());
    container.AddView(_createCategoryGrops(), LPU.LinearMatchWrap());
    container.AddView(_createExcludedKeywords(), LPU.LinearMatchWrap());

    AddView(container, LPU.FrameMatch());

    dataContext.Bind(nameof(ViewerVM.Selected), x => x.Selected, _onSelectedChanged, false);
  }

  private void _onSelectedChanged(ViewerM? viewerM) {
    if (viewerM == null) {
      _includedFolders.SetItems([]);
      _excludedFolders.SetItems([]);
      _excludedKeywords.SetItems([]);
    }
    else {
      _includedFolders.SetItems(viewerM.IncludedFolders);
      _excludedFolders.SetItems(viewerM.ExcludedFolders);
      _excludedKeywords.SetItems(viewerM.ExcludedKeywords);
    }
  }

  private LinearLayout _createIncludedFolders() {
    var container = _createHeaderedContainer(Context!, Common.Res.IconFolder, "Included Folders");
    container.AddView(_includedFolders, LPU.Linear(LPU.Match, DisplayU.DpToPx(200)));

    return container;
  }

  private LinearLayout _createExcludedFolders() {
    var container = _createHeaderedContainer(Context!, Common.Res.IconFolder, "Excluded Folders");
    container.AddView(_excludedFolders, LPU.Linear(LPU.Match, DisplayU.DpToPx(200)));

    return container;
  }

  private LinearLayout _createCategoryGrops() {
    var container = _createHeaderedContainer(Context!, Res.IconGroup, "Category Groups");
    container.AddView(_categoryGroups, LPU.Linear(LPU.Match, DisplayU.DpToPx(200)));

    return container;
  }

  private LinearLayout _createExcludedKeywords() {
    var container = _createHeaderedContainer(Context!, Common.Res.IconTagLabel, "Excluded Keywords");
    container.AddView(_excludedKeywords, LPU.Linear(LPU.Match, DisplayU.DpToPx(200)));

    return container;
  }

  private static LinearLayout _createHeaderedContainer(Context context, string? iconName, string text) {
    var header = new IconTextView(context, iconName, text) {
      Background = BackgroundFactory.Dark()
    }.WithPadding(DimensU.Spacing);

    return LayoutU.Vertical(context)
      .Add(header, LPU.LinearMatchWrap());
  }
}