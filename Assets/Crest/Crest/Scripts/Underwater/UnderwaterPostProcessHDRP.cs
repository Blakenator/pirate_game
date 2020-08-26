// Crest Ocean System for HDRP

// Copyright 2020 Wave Harmonic Ltd

using System.Reflection;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Experimental.Rendering;
using static Crest.UnderwaterPostProcessUtils;

namespace Crest
{
    [VolumeComponentMenu("Crest/Underwater")]
    public class UnderwaterPostProcessHDRP : CustomPostProcessVolumeComponent, IPostProcessComponent
    {
        public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

        private Material _underwaterPostProcessMaterial;
        private PropertyWrapperMaterial _underwaterPostProcessMaterialWrapper;
        private Material _oceanMaskMaterial;

        private const string SHADER = "Hidden/Crest/Underwater/Post Process HDRP";
        private const string SHADER_OCEAN_MASK = "Crest/Underwater/Ocean Mask";

        RTHandle _textureMask;
        RTHandle _depthBuffer;
        RTHandle _cameraDepthTexture;

        bool _firstRender = true;
        public BoolParameter _enable = new BoolParameter(true);
        public BoolParameter _copyOceanMaterialParamsEachFrame = new BoolParameter(false);

        [Header("Debug Options")]
        public BoolParameter _viewOceanMask = new BoolParameter(false);
        public BoolParameter _disableOceanMask = new BoolParameter(false);
        [Tooltip(UnderwaterPostProcessUtils.tooltipHorizonSafetyMarginMultiplier)]
        public ClampedFloatParameter _horizonSafetyMarginMultiplier = new ClampedFloatParameter(UnderwaterPostProcessUtils.DefaultHorizonSafetyMarginMultiplier, 0f, 1f);

        UnderwaterSphericalHarmonicsData _sphericalHarmonicsData = new UnderwaterSphericalHarmonicsData();
        Camera _camera;

        public bool IsActive()
        {
            if (!Application.isPlaying)
            {
                return false;
            }
            if (OceanRenderer.Instance != null)
            {
                return OceanRenderer.Instance.ViewerHeightAboveWater < 2f && _enable.value;
            }
            else
            {
                return false;
            }
        }

        public override void Setup()
        {
            _textureMask = RTHandles.Alloc(
                scaleFactor: Vector2.one,
                slices: TextureXR.slices,
                dimension: TextureXR.dimension,
                colorFormat: GraphicsFormat.R16_SFloat,
                useDynamicScale: true,
                name: "Ocean Mask"
            );

            _depthBuffer = RTHandles.Alloc(
                scaleFactor: Vector2.one,
                slices: TextureXR.slices,
                dimension: TextureXR.dimension,
                depthBufferBits: DepthBits.Depth24,
                colorFormat: GraphicsFormat.R8_UNorm, // This appears to be used for depth.
                enableRandomWrite: false,
                useDynamicScale: true,
                name: "Ocean Mask Depth"
            );

            SetupCameraDepthTexture();
            _underwaterPostProcessMaterial = CoreUtils.CreateEngineMaterial(SHADER);
            _underwaterPostProcessMaterialWrapper = new PropertyWrapperMaterial(_underwaterPostProcessMaterial);
            _oceanMaskMaterial = CoreUtils.CreateEngineMaterial(SHADER_OCEAN_MASK);
            RenderPipelineManager.beginCameraRendering += UnderwaterPostProcessMaskRenderer.BeginCameraRendering;
        }

        public override void Cleanup()
        {
            RenderPipelineManager.beginCameraRendering -= UnderwaterPostProcessMaskRenderer.BeginCameraRendering;
            // We don't have control of the setup/cleanup lifecycle of the post-process effect, so destroying the mesh
            // renderer is the best solution as we don't know when we will ever use it again.
            if (_camera) Destroy(_camera.GetComponent<UnderwaterPostProcessMaskRenderer>());
            CoreUtils.Destroy(_underwaterPostProcessMaterial);
            CoreUtils.Destroy(_oceanMaskMaterial);
            _textureMask.Release();
            _depthBuffer.Release();
        }

        void SetupCameraDepthTexture()
        {
            // We have to use reflection or a custom pass to get the depth texture without transparent objects.
            // Reflection seems simpler for the time being.
            HDRenderPipeline hdRenderPipeline = (HDRenderPipeline)RenderPipelineManager.currentPipeline;
            FieldInfo sharedRTManagerFieldInfo = hdRenderPipeline.GetType()
                .GetField("m_SharedRTManager", BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo getDepthTextureMethodInfo = sharedRTManagerFieldInfo.FieldType.GetMethod("GetDepthTexture");
            // We have to use the method to get the RTHandle since the m_SharedRTManager is internal so we cannot use
            // its type?
            _cameraDepthTexture = (RTHandle)getDepthTextureMethodInfo
                .Invoke(sharedRTManagerFieldInfo.GetValue(hdRenderPipeline), new object[] { false });
        }

        public override void Render(CommandBuffer commandBuffer, HDCamera camera, RTHandle source, RTHandle destination)
        {
            // Used for cleanup
            _camera = camera.camera;

            // TODO: Put somewhere else?
            XRHelpers.Update(_camera);

            if (OceanRenderer.Instance == null)
            {
                HDUtils.BlitCameraTexture(commandBuffer, source, destination);
                return;
            }

            // Applying the post-processing effect to the scene camera doesn't
            // work well.
            if (camera.camera.gameObject.name == "SceneCamera")
            {
                HDUtils.BlitCameraTexture(commandBuffer, source, destination);
                return;
            }

            UnderwaterPostProcessMaskRenderer perCameraData = camera.camera.GetComponent<UnderwaterPostProcessMaskRenderer>();
            if (perCameraData == null)
            {
                perCameraData = camera.camera.gameObject.AddComponent<UnderwaterPostProcessMaskRenderer>();
                // NOTE: Patch RTHandle. Would need refactor.
                perCameraData._textureMask = _textureMask;
                perCameraData._depthBuffer = _depthBuffer;
                perCameraData.Initialise(_oceanMaskMaterial, ((RenderTexture)source).descriptor, _disableOceanMask);
                HDUtils.BlitCameraTexture(commandBuffer, source, destination);
                return;
            }

            UpdatePostProcessMaterial(
                source,
                camera.camera,
                _underwaterPostProcessMaterialWrapper,
                _sphericalHarmonicsData,
                perCameraData._sampleHeightHelper,
                _firstRender || _copyOceanMaterialParamsEachFrame.value,
                _viewOceanMask.value,
                _horizonSafetyMarginMultiplier.value
            );

            // In HDRP we get given a depth buffer which contains the depths of rendered transparencies
            // (such as the ocean). We would preferably only have opaque objects in the depth buffer, so that we can
            // more easily tell whether the current pixel is rendering the ocean surface or not.
            // (We would do this by checking if the ocean mask pixel is in front of the scene pixel.)
            // - Tom Read Cutting - 2020-01-03
            if (_cameraDepthTexture == null) SetupCameraDepthTexture();
            commandBuffer.SetGlobalTexture("_CrestCameraDepthTexture", _cameraDepthTexture);

            HDUtils.DrawFullScreen(commandBuffer, _underwaterPostProcessMaterial, destination);
        }
    }
}
