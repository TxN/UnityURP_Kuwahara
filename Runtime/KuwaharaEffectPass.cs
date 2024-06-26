using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KuwaharaURP {
	[System.Serializable]
	public sealed class KuwaharaEffectPass : ScriptableRenderPass {
		RenderTargetIdentifier _source;
		RenderTargetIdentifier _destinationA;
		RenderTargetIdentifier _destinationB;
		RenderTargetIdentifier _latestDest;

		RenderTargetIdentifier _structureTensor;
		RenderTargetIdentifier _eigenvectors1;
		RenderTargetIdentifier _eigenvectors2;

		readonly int _stensorID		  = Shader.PropertyToID("_StructureTensor");
		readonly int _eigenvectors1ID = Shader.PropertyToID("_Eigenvectors1");
		readonly int _eigenvectors2ID = Shader.PropertyToID("_Eigenvectors2");

		readonly int temporaryRTIdA   = Shader.PropertyToID("_TempRT");
		readonly int temporaryRTIdB   = Shader.PropertyToID("_TempRTB");

		Material		   _effectMaterial;
		KuwaharaEffectType _effectType;

		public KuwaharaEffectPass() {
			renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.depthBufferBits = 0;

			var renderer = renderingData.cameraData.renderer;
			_source = renderer.cameraColorTarget;

			cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
			_destinationA = new RenderTargetIdentifier(temporaryRTIdA);
			cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
			_destinationB = new RenderTargetIdentifier(temporaryRTIdB);

			var stack = VolumeManager.instance.stack;
			var customEffect = stack.GetComponent<KuwaharaEffectPPComponent>();
			if ( customEffect.IsActive() && customEffect.Enabled.value && customEffect.EffectType.value == KuwaharaEffectType.Anisotropic ) {
				cmd.GetTemporaryRT(_stensorID, descriptor, FilterMode.Bilinear);
				_structureTensor = new RenderTargetIdentifier(_stensorID);

				cmd.GetTemporaryRT(_eigenvectors1ID, descriptor, FilterMode.Bilinear);
				_eigenvectors1 = new RenderTargetIdentifier(_eigenvectors2ID);

				cmd.GetTemporaryRT(_eigenvectors1ID, descriptor, FilterMode.Bilinear);
				_eigenvectors2 = new RenderTargetIdentifier(_eigenvectors2ID);
			}
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if ( renderingData.cameraData.isSceneViewCamera ) {
				return;
			}
			
			CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
			cmd.Clear();

			var stack = VolumeManager.instance.stack;

			#region Local Methods
			void BlitTo(Material mat, int pass = 0) {
				var first = _latestDest;
				var last = first == _destinationA ? _destinationB : _destinationA;
				Blit(cmd, first, last, mat, pass);

				_latestDest = last;
			}

			void RenderPasses(int passCount) {
				for ( int i = 0; i < passCount; ++i ) {
					BlitTo(_effectMaterial);
				}
			}

			#endregion

			_latestDest = _source;
			var customEffect = stack.GetComponent<KuwaharaEffectPPComponent>();

			if ( customEffect.IsActive() && customEffect.Enabled.value ) {
				var passCount = customEffect.Passes.value;
				switch ( customEffect.EffectType.value ) {
					case KuwaharaEffectType.Basic:
						SetupBasic(customEffect);
						RenderPasses(passCount);
						break;
					case KuwaharaEffectType.Generalized:
						SetupGeneralized(customEffect);
						RenderPasses(passCount);
						break;
					case KuwaharaEffectType.Anisotropic:
						SetupAnisotropic(customEffect);
						Blit(cmd, _source, _structureTensor, _effectMaterial, 0);
						Blit(cmd, _structureTensor, _eigenvectors1, _effectMaterial, 1);
						Blit(cmd, _eigenvectors1, _eigenvectors2, _effectMaterial, 2);
						cmd.SetGlobalTexture("_TFM", _eigenvectors2);
						for ( int i = 0; i < passCount; ++i ) {
							BlitTo(_effectMaterial, 3);
						}

						break;
				}
			}

			Blit(cmd, _latestDest, _source);

			context.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}

		void SetupBasic(KuwaharaEffectPPComponent ppComponent) {
			if ( _effectType != KuwaharaEffectType.Basic || !_effectMaterial ) {
				if ( _effectMaterial ) {
					Object.Destroy(_effectMaterial);
				}
				_effectMaterial = new Material(Resources.Load<Shader>("Shaders/Kuwahara"));
				_effectType = ppComponent.EffectType.value;
			}
			
			_effectMaterial.SetInt("_KernelSize", ppComponent.KernelSize.value);
			_effectMaterial.SetInt("_MinKernelSize", ppComponent.MinKernelSize.value);
			_effectMaterial.SetInt("_AnimateSize", ppComponent.AnimateKernelSize.value ? 1 : 0);
			_effectMaterial.SetFloat("_SizeAnimationSpeed", ppComponent.SizeAnimationSpeed.value);
			_effectMaterial.SetFloat("_NoiseFrequency", ppComponent.NoiseFrequency.value);
			_effectMaterial.SetInt("_AnimateOrigin", ppComponent.AnimateKernelOrigin.value ? 1 : 0);
		}

		void SetupGeneralized(KuwaharaEffectPPComponent ppComponent) {
			if ( _effectType != KuwaharaEffectType.Generalized || !_effectMaterial ) {
				if ( _effectMaterial ) {
					Object.Destroy(_effectMaterial);
				}
				_effectMaterial = new Material(Resources.Load<Shader>("Shaders/GeneralizedKuwahara"));
				_effectType = ppComponent.EffectType.value;
			}
			_effectMaterial.SetInt("_KernelSize", ppComponent.KernelSize.value);
			_effectMaterial.SetInt("_N", 8);
			_effectMaterial.SetFloat("_Q", ppComponent.Sharpness.value);
			_effectMaterial.SetFloat("_Hardness", ppComponent.Hardness.value);
			_effectMaterial.SetFloat("_ZeroCrossing", ppComponent.ZeroCrossing.value);
			_effectMaterial.SetFloat("_Zeta", ppComponent.UseZeta.value ? ppComponent.Zeta.value : 2.0f / (ppComponent.KernelSize.value / 2.0f));
		}

		void SetupAnisotropic(KuwaharaEffectPPComponent ppComponent) {
			if ( _effectType != KuwaharaEffectType.Anisotropic || !_effectMaterial ) {
				if ( _effectMaterial ) {
					Object.Destroy(_effectMaterial);
				}
				_effectMaterial = new Material(Resources.Load<Shader>("Shaders/AnisotropicKuwahara"));
				_effectType = ppComponent.EffectType.value;
			}
			_effectMaterial.SetInt("_KernelSize", ppComponent.KernelSize.value);
			_effectMaterial.SetInt("_N", 8);
			_effectMaterial.SetFloat("_Q", ppComponent.Sharpness.value);
			_effectMaterial.SetFloat("_Hardness", ppComponent.Hardness.value);
			_effectMaterial.SetFloat("_ZeroCrossing", ppComponent.ZeroCrossing.value);
			_effectMaterial.SetFloat("_Alpha", ppComponent.Alpha.value);
			_effectMaterial.SetFloat("_Zeta", ppComponent.UseZeta.value ? ppComponent.Zeta.value : 2.0f / (ppComponent.KernelSize.value / 2.0f));
		}

		public override void OnCameraCleanup(CommandBuffer cmd) {
			cmd.ReleaseTemporaryRT(temporaryRTIdA);
			cmd.ReleaseTemporaryRT(temporaryRTIdB);

			cmd.ReleaseTemporaryRT(_stensorID);
			cmd.ReleaseTemporaryRT(_eigenvectors1ID);
			cmd.ReleaseTemporaryRT(_eigenvectors2ID);
		}

		public void DeInit() {
			if ( _effectMaterial ) {
				Object.Destroy( _effectMaterial );
				_effectMaterial = null;
			}
		}
	}
}
