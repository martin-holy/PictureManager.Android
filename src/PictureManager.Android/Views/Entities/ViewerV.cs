using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using MH.UI;
using MH.UI.Android.Binding;
using MH.UI.Android.Controls;
using MH.UI.Android.Controls.Items;
using MH.UI.Android.Extensions;
using MH.UI.Android.Utils;
using MH.UI.Android.Views;
using MH.Utils;
using MH.Utils.Disposables;
using MH.Utils.Interfaces;
using PictureManager.Common.Features.Viewer;
using System.Windows.Input;

namespace PictureManager.Android.Views.Entities;

public sealed class ViewerV : ScrollView {
  private readonly SelectableItemsView<IListItem> _includedFolders;
  private readonly SelectableItemsView<IListItem> _excludedFolders;
  private readonly SelectableItemsView<IListItem> _categoryGroups;
  private readonly SelectableItemsView<IListItem> _excludedKeywords;
  private readonly BindingScope _bindings = new();

  public ViewerV(Context context, ViewerVM dataContext) : base(context) {
    _includedFolders = new(context, [], x => new ListItemV(x));
    _excludedFolders = new(context, [], x => new ListItemV(x));
    // TODO 
    _categoryGroups = new(context, dataContext.CategoryGroups, x => new ListItemV(x));
    _categoryGroups.Selection.IsMultiSelect = true;
    _categoryGroups.SetLayoutManager(new LinearLayoutManager(context, LinearLayoutManager.Vertical, false));
    //_categoryGroups.ItemClickedEvent += item => // set IsSelected and call VIewerVM._updateExcludedCategoryGroups
    _excludedKeywords = new(context, [], x => new ListItemV(x));

    var container = LayoutU.Vertical(context)
      .Add(_createIncludedFolders(), LPU.LinearMatchWrap())
      .Add(_createExcludedFolders(), LPU.LinearMatchWrap())
      .Add(_createCategoryGrops(), LPU.LinearMatchWrap())
      .Add(_createExcludedKeywords(), LPU.LinearMatchWrap());

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

  private LinearLayout _createIncludedFolders() =>
    _createContainerWithRemove(Context!, Common.Res.IconFolder, "Included Folders",
      _includedFolders, ViewerVM.RemoveIncludedFolderCommand);

  private LinearLayout _createExcludedFolders() =>
    _createContainerWithRemove(Context!, Common.Res.IconFolder, "Excluded Folders",
      _excludedFolders, ViewerVM.RemoveExcludedFolderCommand);

  private LinearLayout _createExcludedKeywords() =>
    _createContainerWithRemove(Context!, Common.Res.IconTagLabel, "Excluded Keywords",
      _excludedKeywords, ViewerVM.RemoveExcludedKeywordCommand);

  private LinearLayout _createCategoryGrops() =>
    _createContainer(Context!, Res.IconGroup, "Category Groups", _categoryGroups);

  private static LinearLayout _createContainer(Context context, string? iconName, string text,
    SelectableItemsView<IListItem> view) {
    var header = new IconTextView(context, iconName, text) {
      Background = BackgroundFactory.Dark()
    }.WithPadding(DimensU.Spacing);

    return LayoutU.Vertical(context)
      .Add(header, LPU.LinearMatchWrap())
      .Add(view, LPU.Linear(LPU.Match, DisplayU.DpToPx(200)));
  }

  private LinearLayout _createContainerWithRemove(Context context, string? iconName, string text,
    SelectableItemsView<IListItem> view, ICommand command) =>
    _createContainer(context, iconName, text, view)
      .Add(
        new Button(new ContextThemeWrapper(Context, Resource.Style.mh_DialogButton), null, 0)
          .WithClickCommand(command, _bindings, view.Selection.SelectedItem),
        LPU.LinearWrap(GravityFlags.Right).WithMargin(DimensU.Spacing));

  protected override void Dispose(bool disposing) {
    if (disposing) _bindings.Dispose();
    base.Dispose(disposing);
  }
}