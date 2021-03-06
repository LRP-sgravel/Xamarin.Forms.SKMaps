# Xamarin.Forms.SKMaps
## Map pins and overlays for Xamarin.Forms.Maps, drawn with SkiaSharp
**Xamarin.Forms.SKMaps** allows developers an easier integration of custom map features from a shared code base.  Built over Xamarin.Forms.Maps and adding the strength of SkiaSharp rendering, you get easy and highly performant map drawing features from a single code base.

## Build status
| Nuget | iOS | Android |
| ------------- | ------------- | ------------- |
| [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.SKMaps.svg)](https://www.nuget.org/packages/Xamarin.Forms.SKMaps/) | [![Build status](https://build.appcenter.ms/v0.1/apps/0f6577e2-2c35-4491-97b1-e0b0671d5b23/branches/master/badge)](https://appcenter.ms)  | [![Build status](https://build.appcenter.ms/v0.1/apps/f5d64264-dee5-4b06-bfa4-a5fab2633624/branches/master/badge)](https://appcenter.ms)  |

<img alt="Xamarin.Forms.SKMaps Demo" src="./088E2535-1902-47EA-932D-DF9F23A4B8E5.png" width="272" />

# Documentation
Using Xamarin.Forms.SKMaps is as easy as using a regular Xamarin Forms Maps.  You can use the map control in Xaml or code, following your preference:

<table>
  <tr>
    <th>Xaml</th><th>Code</th>
  </tr>
  <tr>
    <td><pre lang=xml>&lt;skmap:SKMap /&gt;</pre></td>
    <td><pre lang="csharp">SKMap map = new SKMap();</pre></td>
  </tr>
</table>
 
if using Xaml, make sure to include the proper namespace in your root element :
```xml
<ContentPage xmlns:skmap="clr-namespace:Xamarin.Forms.SKMaps;assembly=Xamarin.Forms.SKMaps">
  ...
</ContentPage>
```

## Pins
Adding a `SKPin` could not be easier.  It uses the map's regular `Pins` property that you are used to.

<table>
  <tr>
    <th>Xaml</th><th>Code</th>
  </tr>
  <tr>
    <td><pre lang=xml>&lt;skmap:SKMap&gt;
  &lt;skmap:SKMap.Pins&gt;
    &lt;CustomPin Width="32"
               Height="32" /&gt;
  &lt;/skmap:SKMap.Pins&gt;
&lt;/skmap:SKMap&gt;</pre></td>
    <td><pre lang="csharp">map.Pins.Add(new CustomPin());</pre></td>
  </tr>
</table>

### Drawing your pin
The `CustomPin` class in the above example is the class responsible for rendering the pin marker.  To do so, subclass `SKPin` and override the `DrawPin` method.  You will receive a `SKSurface` to draw on.  This surface will be sized according to its `Width`, `Height` and the device's screen density.

```csharp
public override void DrawPin(SKSurface surface)
{
    SKCanvas canvas = surface.Canvas;

    if (_svgIcon != null)
    {
        SKMatrix fillMatrix = GetFillMatrix(canvas, _svgIcon.Picture.CullRect);

        canvas.DrawPicture(_svgIcon.Picture, ref fillMatrix);
    }
}
```

### Other properties
`SKPin` provides other properties to modify it's behavior.  As it subclasses the `Pin` class, you also have access to the regular pin properties.

| Property | Purpose | Default |
| ------------- | ------------- | ------------- |
| `Width` | The width of the pin, in device independent pixels. | 32 dip |
| `Height` | The height of the pin, in device independent pixels. | 32 dip |
| `AnchorX` | Set the pin's anchor X position.  The anchor will be over the pin's position on the map.  This value is in the range { 0...1 } with a value of 0 being at the left. | 0.5 (Center) |
| `AnchorY` | Set the pin's anchor Y position.  The anchor will be over the pin's position on the map.  This value is in the range { 0...1 } with a value of 0 being at the top. | 0.5 (Center) |
| `IsVisible` | Set the pin's visibility on the map. | true (Visible) |
| `Clickable` | Set the pin's clickability on the map. | true (Clickable) |

### Redrawing the pin
To force the pin to be redrawn, you can call the `Invalidate` method.  Changing the `Width` or `Height` will also force a refresh of the pin graphics to accomodate the new size.

## Overlays
The `SKMapOverlay` can be used to add custom "map tile" style overlays.  To do so, add your overlays to the `SKMap` `MapOverlays`property.

<table>
  <tr>
    <th>Xaml</th><th>Code</th>
  </tr>
  <tr>
    <td><pre lang=xml>&lt;skmap:SKMap&gt;
  &lt;skmap:SKMap.MapOverlays&gt;
    &lt;CustomOverlay /&gt;
  &lt;/skmap:SKMap.MapOverlays&gt;
&lt;/skmap:SKMap&gt;</pre></td>
    <td><pre lang="csharp">map.MapOverlays.Add(new CustomOverlay());</pre></td>
  </tr>
</table>

### Drawing your overlay
Similar to the `SKPin`, the `SKMapOverlay` should be subclassed and the overlay is drawn within an override named `DrawOnMap`.  To ease the drawing calculations, all drawing occurs in **GPS coordinates** through the `SKMapCanvas` class and other utility classes like `SKMapPath`.  The `SKPaint` properties remain in pixels, for instance `StrokeWidth`.

```csharp
public override void DrawOnMap(SKMapCanvas canvas, SKMapSpan canvasMapRect, double zoomScale)
{
    if (Route != null && Route.Points.Count() > 1)
    {
        Position firstPoint = Route.Points.FirstOrDefault();
        List<Position> currentRoute = new List<Position>(Route.Points.Skip(1));
        SKMapPath path = canvas.CreateEmptyMapPath();

        path.MoveTo(firstPoint);
        foreach (Position nextPoint in currentRoute)
        {
            path.LineTo(nextPoint);
        }

        using (SKPaint paint = new SKPaint())
        {
            paint.Color = LineColor.ToSKColor();
            paint.StrokeWidth = LineWidth;
            paint.Style = SKPaintStyle.Stroke;
            paint.IsAntialias = true;

            canvas.DrawPath(path, paint);
        }
    }
}
```

#### Notes
- You should always consider the `canvasMapRect` parameter when doing the render calls to avoid drawing outside of the current rendering area.  Failure to do so could result in slower performance.
- This method might be called concurently for many tiles, your implementation therefore needs to be thread-safe.
- `DrawOnMap` will not be called for tiles outside of your overlay's `GpsBounds`.
- Drawing shapes across the 180th meridian will automatically handle the necessary wrap-around.  To cross the 180th meridian,  shapes require passing a `MapSpan` that goes across the 180th meridian, meaning that it would reach longitudes under -180 degrees or above 180 degrees (i.e. A rectangle centered at 179 degrees longitude with a span of 5 degrees).
- In the case of lines, the shortest route between the two provided coordinates is rendered by default. You can opt out and force the long route to be rendered by setting the `shortLine` parameter to `false` in the `DrawLine` calls.

### Other properties
`SKMapOverlay` provides other properties to modify it's behavior.

| Property | Purpose | Default |
| ------------- | ------------- | ------------- |
| `GpsBounds` | Gps bounds that fully emcompass the overlay. Tiles outside this area will not be rendered. | N/A |
| `IsVisible` | Set the overlay's visibility on the map. | true (Visible) |


### Redrawing the overlay
To force the overlay to be redrawn, you can call the `Invalidate` method.  Alternatively, changing the `GPSBounds`property from within the child class will also force a full refresh of the overlay.

### Converting coordinates and pixel sizes
Since the `SKMapOverlay` does all it's drawing operations in pixels but in a GPS coordinate space, you are likely to need to convert from pixel sizes to GPS spans.  A common use-case is wanting to draw a line of a given GPS size (i.e. 100 meters wide) regardless of the zoom level and position.  **Xamarin.Forms.SKMaps** provides utility methods to do those conversions.  Those methods are available on the `SKMapCanvas` class.

| Method | Purpose |
| ------------- | ------------- |
| `PixelsToMaximumMapSpanAtScale` | Returns the maximum span that the provided pixels size requires for a given zoom scale. Because of the mercator projection, the maximum span will always be at the equator since the projection stretches the map at the poles. This method is useful to add padding to a shape when we don't know where it will be rendered.  Using a zoom scale of `SKMapCanvas.MaxZoomScale` will provide the maximum padding required when fully zoomed out. |
| `PixelsToMapSizeAtScale` | Returns the span that the provided pixels size requires at the provided location for a given zoom scale. This method is useful to add padding to a shape when we know where it will be rendered. |


# Sample
The provided sample is abnormally large, it is more than a sample.  This is due to the fact that this Nuget started as a Toptal certification project for Xamarin.  I meant to use the project as a starting point for the Nuget but as I never had the time to fit the certification within my schedule, I forked the project into the current sample.  The idea behind the project was to build a fully cross platform app, meaning I had the objective of doing 100% of the features, including the UI, in shared code.  I still have the objective of writing a Toptal blog post on this as it was requested in my previous [blog post](https://www.toptal.com/mobile/xamarin-mvvmcross-skiasharp-cross-platform).

I'm aware this might create a fair bit of confusion as the sample is not straight to the point, given the requirements of the certification on which I built the foundation.  However, this is also a great opportunty to see different cross platform techniques in Xamarin Forms.  The sample therefore introduces different Forms concepts like markup extensions, using MvvmCross value converters in Forms, Forms effects and behaviors, MvvmCross localization, shared project embedded resources accessed through a resource locator, rendering custom fonts in SkiaSharp, etc.

For those solely interested in the map overlay and pin samples, they can be found [here](https://github.com/LRP-sgravel/Xamarin.Forms.SKMaps/tree/master/sample/Xamarin.Forms.SKMaps.Sample.Forms/Controls/Maps) and they are referenced in the [ActivityPage](https://github.com/LRP-sgravel/Xamarin.Forms.SKMaps/blob/master/sample/Xamarin.Forms.SKMaps.Sample.Forms/Pages/ActivityPage.xaml).
