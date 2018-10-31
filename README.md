# Xamarin.Forms.SKMaps

---
## SkiaSharp drawn map pins and overlays for Xamarin.Forms.Maps
**Xamarin.Forms.SKMaps** allows developers an easier integration of custom map features from a shared code base.  Built over Xamarin.Forms.Maps and adding the strength of SkiaSharp rendering, you can get easy and highly performant map drawing features in shared code.

# Build status
| Nuget | iOS | Android |
| ------------- | ------------- | ------------- |
| [![NuGet](https://img.shields.io/nuget/v/Xamarin.Forms.SKMaps.svg)](https://www.nuget.org/packages/Xamarin.Forms.SKMaps/) | [![Build status](https://build.appcenter.ms/v0.1/apps/0f6577e2-2c35-4491-97b1-e0b0671d5b23/branches/master/badge)](https://appcenter.ms)  | [![Build status](https://build.appcenter.ms/v0.1/apps/f5d64264-dee5-4b06-bfa4-a5fab2633624/branches/master/badge)](https://appcenter.ms)  |
 
# Documentation
Using Xamarin.Forms.SKMaps is as easy as using aregular Xamarin Forms Maps.  You can use the map control in Xaml or code, following your preference:

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
<ContentPage xmlns:skmap="clr-namespace:Xamarin.Forms.SKMaps;assembly=Xamarin.Forms.SKMaps" >
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
The `CustomPin`class in the above example is the class responsible for rendering the pin marker.  To do so, subclass `SKPin` and override the `DrawPin` method.  You wil lreceive a `SKSurface` to draw on.  This surface will be sized according to its `Width` and `Height` and the device density.
```csharp
public override void DrawMarker(SKSurface surface)
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
`SKPin` provides other properties to modify it's behavior.

| Property | Purpose | Default |
| ------------- | ------------- | ------------- |
| `Width` | The width of the pin, in device independent pixels. | 32 dip |
| `Height` | The height of the pin, in device independent pixels. | 32 dip |
| `AnchorX` | Set the pin's anchor X position.  The anchor will be over the pin's position on the map.  This value is in the range { 0...1 } with a value of 0 being on the left. | 0.5 (Center) |
| `AnchorY` | Set the pin's anchor Y position.  The anchor will be over the pin's position on the map.  This value is in the range { 0...1 } with a value of 0 being on the top. | 0.5 (Center) |
| `IsVisible` | Set the pin's visibility on the map. | true (Visible) |
| `Clickable` | Set the pin's clickability on the map. | true (Clickable) |

### Redrawing the pin
To force the pin to be redrawn, you can call the `Invalidate` method.

## Overlays
The `SKMapOverlay` can be used to add custom "map tile" style overlays.  To do so, add your overlays to the `SKMap` `MapOverlay`property.

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
Similar to the `SKPin`, the `SKMapOverlay` is drawn within an override named `DrawOnMap`.  To ease the drawing calculations, all drawing occurs in **GPS coordinates** through the `SKMapCanvas` class and other utility classes.

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

### Other properties
`SKMapOverlay` provides other properties to modify it's behavior.

| Property | Purpose | Default |
| ------------- | ------------- | ------------- |
| `GpsBounds` | Gps bounds that fully emcompass the overlay. | --- |
| `IsVisible` | Set the overlay's visibility on the map. | true (Visible) |


### Redrawing the overlay
To force the overlay to be redrawn, you can call the `Invalidate` method.  Alternatively, changing the `GPSBounds`property will also force a full refresh of the overlay.
