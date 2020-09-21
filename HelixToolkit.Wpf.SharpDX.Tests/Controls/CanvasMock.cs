﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CanvasMock.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using SharpDX;
using SharpDX.Direct3D11;

namespace HelixToolkit.Wpf.SharpDX.Tests.Controls
{
    class CanvasMock : IRenderHost
    {
        public CanvasMock()
        {
            RenderTechniquesManager = new DefaultRenderTechniquesManager();
            RenderTechnique = RenderTechniquesManager.RenderTechniques[DefaultRenderTechniqueNames.Phong];
            EffectsManager = new DefaultEffectsManager(RenderTechniquesManager);
            Device = EffectsManager.Device;
        }

        public Device Device { get; private set; }
        public Color4 ClearColor { get; private set; }
        public bool IsShadowMapEnabled { get; private set; }
        public bool IsMSAAEnabled { get; private set; }
        public IRenderer Renderable { get; set; }
        public RenderTechnique RenderTechnique { get; private set; }
        public double ActualHeight { get; private set; }
        public double ActualWidth { get; private set; }

        public IEffectsManager EffectsManager { get; set; }

        public IRenderTechniquesManager RenderTechniquesManager { get; set; }

        public void SetDefaultRenderTargets()
        {
        }

        public void SetDefaultColorTargets(DepthStencilView dsv)
        {
        }

        public event EventHandler<SharpDX.Utilities.RelayExceptionEventArgs> ExceptionOccurred;

        public void InvalidateRender()
        {
        }
    }
}
