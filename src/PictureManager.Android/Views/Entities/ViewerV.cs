using Android.Content;
using Android.Views;
using Android.Widget;
using MH.UI;
using MH.UI.Android.Binding;
using MH.UI.Android.Controls;
using MH.UI.Android.Controls.Items;
using MH.UI.Android.Extensions;
using MH.UI.Android.Utils;
using MH.UI.Android.Views;
using MH.Utils;
using MH.Utils.Disposables;
using MH.Utils.Extensions;
using MH.Utils.Interfaces;
using PictureManager.Common.Features.Viewer;
using System.Linq;

namespace PictureManager.Android.Views.Entities;

public sealed class ViewerV : ScrollView {
  private readonly SelectableItemsView<IListItem> _includedFolders;
  private readonly SelectableItemsView<IListItem> _excludedFolders;
  private readonly SelectableItemsView<IListItem> _categoryGroups;
  private readonly SelectableItemsView<IListItem> _excludedKeywords;
  private readonly BindingScope _bindings = new();
  private CommandBinding _includedFoldersRemoveBinding = null!;
  private CommandBinding _excludedFoldersRemoveBinding = null!;
  private CommandBinding _excludedKeywordsRemoveBinding = null!;
  private bool _categoryGroupsUpdating;
  private readonly ViewerVM _dataContext;

  public ViewerV(Context context, ViewerVM dataContext) : base(context) {
    _dataContext = dataContext;

    _includedFolders = new(context, [], x => new ListItemV(x));
    _includedFolders.Selection.ItemSelectionChanged += _incudedFoldersItemSelectionChanged;

    _excludedFolders = new(context, [], x => new ListItemV(x));
    _excludedFolders.Selection.ItemSelectionChanged += _excudedFoldersItemSelectionChanged;

    _categoryGroups = new(context, dataContext.CategoryGroups, x => new ListItemV(x));
    _categoryGroups.Selection.IsMultiSelect = true;
    _categoryGroups.Selection.ItemSelectionChanged += _categoryGroupsItemSelectionChanged;

    _excludedKeywords = new(context, [], x => new ListItemV(x));
    _excludedKeywords.Selection.ItemSelectionChanged += _excludedKeywordsItemSelectionChanged;

    var container = LayoutU.Vertical(context)
      .Add(_createIncludedFolders(), LPU.LinearMatchWrap())
      .Add(_createExcludedFolders(), LPU.LinearMatchWrap())
      .Add(_createCategoryGrops(), LPU.LinearMatchWrap())
      .Add(_createExcludedKeywords(), LPU.LinearMatchWrap());

    AddView(container, LPU.FrameMatch());

    dataContext.Bind(nameof(ViewerVM.Selected), x => x.Selected, _onSelectedChanged);
  }

  private void _incudedFoldersItemSelectionChanged(IListItem item, bool selected) =>
    _includedFoldersRemoveBinding.Parameter = selected ? item : null;

  private void _excudedFoldersItemSelectionChanged(IListItem item, bool selected) =>
    _excludedFoldersRemoveBinding.Parameter = selected ? item : null;

  private void _categoryGroupsItemSelectionChanged(IListItem item, bool selected) {
    if (_categoryGroupsUpdating) return;
    item.IsSelected = selected;
    _dataContext.UpdateExcludedCategoryGroupsCommand.Execute(null);
  }

  private void _excludedKeywordsItemSelectionChanged(IListItem item, bool selected) =>
    _excludedKeywordsRemoveBinding.Parameter = selected ? item : null;

  private void _onSelectedChanged(ViewerM? viewerM) {
    if (viewerM == null) return;
    ScrollTo(0, 0);
    _includedFolders.SetItems(viewerM.IncludedFolders);
    _excludedFolders.SetItems(viewerM.ExcludedFolders);
    _excludedKeywords.SetItems(viewerM.ExcludedKeywords);
    _categoryGroups.Post(() => _updateCategoryGroups(viewerM));
    
    Post(() => {
      _includedFoldersRemoveBinding.Parameter = null;
      _excludedFoldersRemoveBinding.Parameter = null;
      _excludedKeywordsRemoveBinding.Parameter = null;
    });
  }

  private void _updateCategoryGroups(ViewerM viewerM) {
    try {
      _categoryGroupsUpdating = true;
      foreach (var cg in _dataContext.CategoryGroups) {
        _categoryGroups.Selection.Select(cg);
        cg.IsSelected = !viewerM.ExcludedCategoryGroups.Contains(cg.Data);
        if (!cg.IsSelected) _categoryGroups.Selection.Deselect(cg);
      }
    }
    finally {
      _categoryGroupsUpdating = false;
    }
  }

  private LinearLayout _createIncludedFolders() {
    var layout = _createContainerWithRemove(Context!, Common.Res.IconFolder, "Included Folders", _includedFolders, out var btn);
    _includedFoldersRemoveBinding = new CommandBinding(btn).Bind(ViewerVM.RemoveIncludedFolderCommand);
    return layout;
  }

  private LinearLayout _createExcludedFolders() {
    var layout = _createContainerWithRemove(Context!, Common.Res.IconFolder, "Excluded Folders", _excludedFolders, out var btn);
    _excludedFoldersRemoveBinding = new CommandBinding(btn).Bind(ViewerVM.RemoveExcludedFolderCommand);
    return layout;
  }

  private LinearLayout _createExcludedKeywords() {
    var layout = _createContainerWithRemove(Context!, Common.Res.IconTagLabel, "Excluded Keywords", _excludedKeywords, out var btn);
    _excludedKeywordsRemoveBinding = new CommandBinding(btn).Bind(ViewerVM.RemoveExcludedKeywordCommand);
    return layout;
  }

  private LinearLayout _createCategoryGrops() =>
    _createContainer(Context!, Res.IconGroup, "Category Groups", _categoryGroups);

  private static LinearLayout _createContainer(Context context, string? iconName, string text,
    SelectableItemsView<IListItem> view) {
    var header = new IconTextView(context, iconName, text) {
      Background = BackgroundFactory.Dark()
    }.WithPadding(DimensU.Spacing);

    return LayoutU.Vertical(context)
      .Add(header, LPU.LinearMatchWrap())
      .Add(view, LPU.Linear(LPU.Match, DisplayU.DpToPx(150)));
  }

  private LinearLayout _createContainerWithRemove(Context context, string? iconName, string text,
    SelectableItemsView<IListItem> view, out Button btn) {

    btn = new Button(new ContextThemeWrapper(Context, Resource.Style.mh_DialogButton), null, 0);

    return _createContainer(context, iconName, text, view)
      .Add(btn, LPU.LinearWrap(GravityFlags.Right).WithMargin(DimensU.Spacing));
  }

  protected override void Dispose(bool disposing) {
    if (disposing) {
      _bindings.Dispose();
      _includedFolders.Selection.ItemSelectionChanged -= _incudedFoldersItemSelectionChanged;
      _excludedFolders.Selection.ItemSelectionChanged -= _excudedFoldersItemSelectionChanged;
      _categoryGroups.Selection.ItemSelectionChanged -= _categoryGroupsItemSelectionChanged;
      _excludedKeywords.Selection.ItemSelectionChanged -= _excludedKeywordsItemSelectionChanged;
    }
    base.Dispose(disposing);
  }
}