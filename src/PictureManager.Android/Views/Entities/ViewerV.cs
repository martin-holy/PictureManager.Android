using Android.Content;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using MH.UI.Android.Controls.Items;
using MH.UI.Android.Utils;
using MH.UI.Android.Views;
using MH.Utils.BaseClasses;
using PictureManager.Common.Features.CategoryGroup;
using PictureManager.Common.Features.Viewer;

namespace PictureManager.Android.Views.Entities;

public sealed class ViewerV : ScrollView {
  private readonly SelectableItemsView<ListItem<CategoryGroupM>> _categoryGroups;

  public ViewerV(Context context, ViewerVM dataContext) : base(context) {
    var container = LayoutU.Vertical(context);
    // TODO 
    _categoryGroups = new(context, dataContext.CategoryGroups, x => new ListItemV(x));
    _categoryGroups.SetLayoutManager(new LinearLayoutManager(context, LinearLayoutManager.Vertical, false));
    //_categoryGroups.ItemClickedEvent += item => // set IsSelected and call VIewerVM._updateExcludedCategoryGroups

    container.AddView(_categoryGroups, LPU.LinearMatchWrap());

    AddView(container, LPU.FrameWrap());
  }
}