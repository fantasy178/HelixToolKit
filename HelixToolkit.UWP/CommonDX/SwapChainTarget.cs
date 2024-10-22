﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HelixToolkit.UWP.CommonDX
{
    public class SwapChainTarget : TargetBase
    {
        private Windows.UI.Xaml.Controls.SwapChainPanel swapChainPanel;

        private int pixelWidth;
        private int pixelHeight;

        private SharpDX.Direct3D11.DeviceContext1 deviceContext;
        private SharpDX.DXGI.SwapChain2 swapChain;

        public SwapChainTarget(Windows.UI.Xaml.Controls.SwapChainPanel swapChainPanel, int pixelWidth, int pixelHeight)
        {
            this.swapChainPanel = swapChainPanel;

            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
        }

        protected override Rect CurrentControlBounds
        {
            get
            {
                return new Windows.Foundation.Rect(0, 0, pixelWidth, pixelHeight); 
            }
        }

        public override void Initialize(DeviceManager deviceManager)
        {
            base.Initialize(deviceManager);

            // Save the context instance
            this.deviceContext = deviceManager.DeviceDirect3D.ImmediateContext1;

            // We have to take into account pixel scaling; Windows Phone 8.1 uses virtual resolutions smaller than the physical screen size.
            float pixelScale = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi / 96.0f;

            // Properties of the swap chain
            SharpDX.DXGI.SwapChainDescription1 swapChainDescription = new SharpDX.DXGI.SwapChainDescription1()
            {
                // No transparency.
                AlphaMode = SharpDX.DXGI.AlphaMode.Ignore,
                // Double buffer.
                BufferCount = 2,
                // BGRA 32bit pixel format.
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                // Unlike in CoreWindow swap chains, the dimensions must be set.
                Height = (int)(this.swapChainPanel.RenderSize.Height * pixelScale),
                Width = (int)(this.swapChainPanel.RenderSize.Width * pixelScale),
                // Default multisampling.
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                // In case the control is resized, stretch the swap chain accordingly.
                Scaling = SharpDX.DXGI.Scaling.Stretch,
                // No support for stereo display.
                Stereo = false,
                // Sequential displaying for double buffering.
                SwapEffect = SharpDX.DXGI.SwapEffect.FlipSequential,
                // This swapchain is going to be used as the back buffer.
                Usage = SharpDX.DXGI.Usage.BackBuffer | SharpDX.DXGI.Usage.RenderTargetOutput,
            };

            // Retrive the DXGI device associated to the Direct3D device.
            using (SharpDX.DXGI.Device3 dxgiDevice3 = deviceManager.DeviceDirect3D.QueryInterface<SharpDX.DXGI.Device3>())
            {
                // Get the DXGI factory automatically created when initializing the Direct3D device.
                using (SharpDX.DXGI.Factory3 dxgiFactory3 = dxgiDevice3.Adapter.GetParent<SharpDX.DXGI.Factory3>())
                {
                    // Create the swap chain and get the highest version available.
                    using (SharpDX.DXGI.SwapChain1 swapChain1 = new SharpDX.DXGI.SwapChain1(dxgiFactory3, deviceManager.DeviceDirect3D, ref swapChainDescription))
                    {
                        this.swapChain = swapChain1.QueryInterface<SharpDX.DXGI.SwapChain2>();
                    }
                }
            }

            // Obtain a reference to the native COM object of the SwapChainPanel.
            using (SharpDX.DXGI.ISwapChainPanelNative nativeObject = SharpDX.ComObject.As<SharpDX.DXGI.ISwapChainPanelNative>(this.swapChainPanel))
            {
                // Set its swap chain.
                nativeObject.SwapChain = this.swapChain;
            }

            // Create a descriptor for the depth/stencil buffer.
            // Allocate a 2-D surface as the depth/stencil buffer.
            // Create a DepthStencil view on this surface to use on bind.
            // TODO: Recreate a DepthStencilBuffer is inefficient. We should only have one depth buffer. Shared depth buffer?
            using (var depthBuffer = new SharpDX.Direct3D11.Texture2D(DeviceManager.DeviceDirect3D, new SharpDX.Direct3D11.Texture2DDescription()
            {
                Format = SharpDX.DXGI.Format.D24_UNorm_S8_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = (int)swapChainDescription.Width,
                Height = (int)swapChainDescription.Height,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                BindFlags = SharpDX.Direct3D11.BindFlags.DepthStencil,
            }))

            this.depthStencilView = Collect(new SharpDX.Direct3D11.DepthStencilView(DeviceManager.DeviceDirect3D, depthBuffer, new SharpDX.Direct3D11.DepthStencilViewDescription() { Dimension = SharpDX.Direct3D11.DepthStencilViewDimension.Texture2D }));

            // Create a Texture2D from the existing swap chain to use as 
            this.backBuffer = SharpDX.Direct3D11.Texture2D.FromSwapChain<SharpDX.Direct3D11.Texture2D>(this.swapChain, 0);
            this.renderTargetView = new SharpDX.Direct3D11.RenderTargetView(deviceManager.DeviceDirect3D, this.backBuffer);

            var viewport = new SharpDX.ViewportF(0, 0, (float)swapChainDescription.Width, (float)swapChainDescription.Height, 0.0f, 1.0f);

            RenderTargetBounds = new Rect(viewport.X, viewport.Y, viewport.Width, viewport.Height);

            //DeviceManager.ContextDirect2D.Target = this.backBuffer.as;

            DeviceManager.ContextDirect3D.Rasterizer.SetViewport(viewport);
        }

        public override void RenderAll()
        {
            // Set the active back buffer and clear it.
            this.deviceContext.OutputMerger.SetRenderTargets(this.renderTargetView);
            //this.deviceContext.ClearRenderTargetView(this.backBufferView, SharpDX.Color.CornflowerBlue);

            base.RenderAll();

            // Tell the swap chain to present the buffer.
            this.swapChain.Present(1, SharpDX.DXGI.PresentFlags.None, new SharpDX.DXGI.PresentParameters());
        }
    }
}
