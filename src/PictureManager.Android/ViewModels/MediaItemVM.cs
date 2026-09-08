using Android.Content;
using Android.Graphics;
using Android.Widget;
using MH.UI.Android.Utils;
using PictureManager.Common;
using PictureManager.Common.Features.MediaItem;
using PictureManager.Common.Features.MediaItem.Image;
using PictureManager.Common.Features.MediaItem.Video;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MHOrientation = MH.Utils.Imaging.Orientation;

namespace PictureManager.Android.ViewModels;

public static class MediaItemVM {
  public static Task<Bitmap?> GetFullImage(MediaItemM mi, CancellationToken token) =>
    Task.Run(() => {
      token.ThrowIfCancellationRequested();
      var bmp = BitmapFactory.DecodeFile(mi.FilePath);
      token.ThrowIfCancellationRequested();
      return bmp?.ApplyOrientation(mi.Orientation);
    }, token);

  public static async Task LoadThumb(MediaItemM mi, ImageView imageView, Context context, CancellationToken token) {
    try {
      var bitmap = mi switch {
        ImageM => await GetImageThumb(mi, context, token),
        VideoM => await GetVideoThumb(mi, context, token),
        _ => null
      };

      if (token.IsCancellationRequested) return;

      imageView.Post(() => {
        if (!token.IsCancellationRequested)
          imageView.SetImageBitmap(bitmap);
      });
    }
    catch (OperationCanceledException) {
      // ignored
    }
  }

  public static Task<Bitmap?> GetMediaStoreImageThumb(MediaItemM mi, Context context) =>
    MediaStoreU.GetImageThumbnail(mi.FilePath, context, 512);

  public static Task<Bitmap?> GetMediaStoreVideoThumb(MediaItemM mi, Context context) =>
    MediaStoreU.GetVideoThumbnail(mi.FilePath, context, 512);

  public static Bitmap? CreateImageThumb(MediaItemM mi, int thumbSize) =>
    ImagingU.CreateImageThumbnail(mi.FilePath, thumbSize)?.ApplyOrientation(mi.Orientation);

  public static Bitmap? CreateVideoThumb(MediaItemM mi, int thumbSize) =>
    ImagingU.CreateVideoThumbnail(mi.FilePath, thumbSize);

  public static Task<Bitmap?> GetImageThumb(MediaItemM mi, Context context, CancellationToken token) =>
    GetThumb(mi, context, GetMediaStoreImageThumb, CreateImageThumb, token);

  public static Task<Bitmap?> GetVideoThumb(MediaItemM mi, Context context, CancellationToken token) =>
    GetThumb(mi, context, GetMediaStoreVideoThumb, CreateVideoThumb, token);

  public static async Task<Bitmap?> GetThumb(
    MediaItemM mi,
    Context context,
    Func<MediaItemM, Context, Task<Bitmap?>> getMediaStoreThumb,
    Func<MediaItemM, int, Bitmap?> createThumb,
    CancellationToken token) {

    return await Task.Run(async () => {
      try {
        token.ThrowIfCancellationRequested();

        // 1. Custom cache
        var cachePath = mi.FilePathCache;
        if (File.Exists(cachePath) && BitmapFactory.DecodeFile(cachePath) is { } cached)
          return cached;

        token.ThrowIfCancellationRequested();

        Bitmap? thumb = null;

        // 2. MediaStore only for normal orientation
        if (mi.Orientation == MHOrientation.Normal)
          thumb = await getMediaStoreThumb(mi, context);

        token.ThrowIfCancellationRequested();

        // 3. Custom cache fallback
        if (thumb == null) {
          thumb = createThumb(mi, Core.Settings.MediaItem.ThumbSize);

          if (thumb != null)
            _saveThumbToCache(thumb, cachePath);
        }

        token.ThrowIfCancellationRequested();

        return thumb;
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception ex) {
        MH.Utils.Log.Error(ex);
        return null;
      }
    }, token);
  }

  private static readonly ConcurrentDictionary<string, object> _thumbLocks = new();

  private static void _saveThumbToCache(Bitmap thumb, string cachePath) {
    var folder = System.IO.Path.GetDirectoryName(cachePath);

    if (!string.IsNullOrWhiteSpace(folder))
      Directory.CreateDirectory(folder);

    var sync = _thumbLocks.GetOrAdd(cachePath, _ => new object());

    lock (sync) {
      if (File.Exists(cachePath)) return;
      using var stream = File.Open(cachePath, FileMode.Create, FileAccess.Write, FileShare.Read);
      thumb.Compress(Bitmap.CompressFormat.Jpeg!, 80, stream);
    }
  }
}