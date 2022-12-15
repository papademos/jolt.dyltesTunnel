using Jolt.Math;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static Jolt.Math.MathF;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using D2DFactory = SharpDX.Direct2D1.Factory;
using Device = SharpDX.Direct3D11.Device;
using DxgiFactory1 = SharpDX.DXGI.Factory1;
using Vector4 = System.Numerics.Vector4;

// Create a Form for rendering.
var form = new RenderForm {
    Text = "Joltprickar till Dylte",
    Icon = null,
    WindowState = FormWindowState.Maximized
};

// Listen to keyboard events and close the form on escape.
form.KeyDown += (source, args) => {
    switch (args.KeyCode) {
        case Keys.Escape:
            form.Dispose();
            break;
    }
};

// Create factories needed throughout the application.
using var dxgiFactory = new DxgiFactory1();
using var d2dFactory = new D2DFactory();

// Create the Device, which is our main interface to DirectX.
using var device = new Device(
    DriverType.Hardware,
    DeviceCreationFlags.BgraSupport,
    new SharpDX.Direct3D.FeatureLevel[] { SharpDX.Direct3D.FeatureLevel.Level_11_1 });

// Create a SwapChain so we have something to render to.
using var swapChain = new SwapChain(dxgiFactory, device, new SwapChainDescription {
    BufferCount = 1,
    ModeDescription = new ModeDescription(
        form.ClientSize.Width,
        form.ClientSize.Height,
        new Rational(60, 1),
        Format.R8G8B8A8_UNorm),
    IsWindowed = true,
    OutputHandle = form.Handle,
    SampleDescription = new SampleDescription(1, 0),
    SwapEffect = SwapEffect.Discard,
    Usage = Usage.RenderTargetOutput
});

// Resources needed for rendering to the SwapChain buffers.
var backBuffer = null as Texture2D;
var surface = null! as Surface;
var renderTarget = null! as RenderTarget;

// Calculate the positions for a thorus.
var thorus = CreateThorus(96, 1000, 32, 100);

// Create a stopwatch so we can have something to track time.
var stopwatch = Stopwatch.StartNew();

// Main loop. (RenderFrame is incoked once per frame.)
RenderLoop.Run(form, RenderFrame);

// Clean up resources and quit the application.
renderTarget?.Dispose();
surface?.Dispose();
backBuffer?.Dispose();
device.ImmediateContext.ClearState();
device.ImmediateContext.Flush();
return;

void RenderFrame() {
    var t = (float)stopwatch.Elapsed.TotalSeconds;
    var (w, h) = (form.ClientSize.Width, form.ClientSize.Height);

    UpdateResources(w, h, d2dFactory, swapChain,  ref backBuffer, ref surface, ref renderTarget);
    UpdateGeometry(t, w, h, thorus, out var positions);
    Render(d2dFactory, renderTarget!, positions);
    
    // Copy the rendered result to the screen.
    swapChain.Present(0, PresentFlags.None);
};

static void UpdateGeometry(float t, float w, float h, Vector4[] thorus, out Vector4[] positions) {
    var aspect = 1f * w / h;
    var znear = 10f;
    var zfar = 1001f;

    Vector4 TransformToScreen(Vector4 p) {
        p.X /= p.W;
        p.Y /= p.W;
        p.X = p.X * w + w / 2;
        p.Y = p.Y * h + h / 2;
        return p;
    };
    
    positions = thorus
        .Transform(
            RotX(0.1f * t) *
            Translate(0, 900, 500) *
            RotZ(0.15f * t))
        .Where(p => p.Z > 100)
        .Transform(Perspective(10 * aspect, 10, znear, zfar))
        .Select(TransformToScreen)
        .ToArray();
}

static void UpdateResources(int w, int h, 
    D2DFactory factory,
    SwapChain swapChain, 
    ref Texture2D? backBuffer, 
    ref Surface? surface, 
    ref RenderTarget? renderTarget) {

    var isDirty = 
        w != swapChain.Description.ModeDescription.Width ||
        h != swapChain.Description.ModeDescription.Height;
    if (isDirty) {
        // Dispose old resources.
        Utilities.Dispose(ref renderTarget);
        Utilities.Dispose(ref surface);
        Utilities.Dispose(ref backBuffer);

        // Resize or Create SwapChain.
        swapChain.ResizeBuffers(swapChain.Description.BufferCount, w, h, Format.Unknown, SwapChainFlags.None);
    }

    // (Re)Create all resources needed to get a RenderTarget.
    backBuffer ??= Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
    surface ??= backBuffer.QueryInterface<Surface>();
    renderTarget ??= new RenderTarget(factory, surface, new(new(Format.Unknown, AlphaMode.Premultiplied)));
}

static void Render(D2DFactory factory, RenderTarget renderTarget, Vector4[] positions) {
    renderTarget.BeginDraw();
    renderTarget.Clear(Color.Black);
    foreach (var p in positions) {
        var (x, y) = (p.X, p.Y);
        var s = Saturate(1 - p.Z / -1400);
        var radius = s * 10;
        var color = new Color4(0, 1, 1, s);
        FillEllipse(factory, renderTarget, color, x, y, radius, radius);
    }
    renderTarget.EndDraw();
}

static Vector4[] CreateCircle(int count, float radius)
    => Enumerable
    .Range(0, count)
    .Select(i => (float)i / count)
    .Select(a => new Vector4(radius * Cos(a), radius * Sin(a), 0, 1))
    .ToArray();

static Vector4[] CreateThorus(int largeCount, float largeRadius, int smallCount, float smallRadius)
    => Enumerable
    .Repeat(CreateCircle(smallCount, smallRadius), largeCount)
    .SelectMany((circle, i) => circle.Transform(
        Translate(0, largeRadius, 0) *
        RotX((float)i / largeCount)))
    .ToArray();

static void FillEllipse(D2DFactory factory, RenderTarget target, Color4 color, float x, float y, float radiusX, float radiusY) {
    using var brush = new SolidColorBrush(target, color);
    using var geometry = new EllipseGeometry(factory, new(new(x, y), radiusX, radiusY));
    target.FillGeometry(geometry, brush, null);
}
