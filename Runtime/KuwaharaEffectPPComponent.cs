using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace KuwaharaURP {
	[Serializable, VolumeComponentMenuForRenderPipeline("Custom/Kuwahara", typeof(UniversalRenderPipeline))]
	public sealed class KuwaharaEffectPPComponent : VolumeComponent, IPostProcessComponent {
		public bool IsActive() => active;

		public bool IsTileCompatible() => true;

		public VolumeParameter<bool>  Enabled = new VolumeParameter<bool>();

		[Tooltip("Type of Kuwahara effect implementation")]
		public VolumeParameter<KuwaharaEffectType> EffectType = new VolumeParameter<KuwaharaEffectType> { value = KuwaharaEffectType.Basic };

		public ClampedIntParameter Passes = new ClampedIntParameter(1, 1, 4) { value = 1 };

		[Header("Seiitngs for Basic type:")]
		public ClampedFloatParameter NoiseFrequency			= new ClampedFloatParameter(10f, 0f, 30f) { value = 10.0f };

		public ClampedIntParameter   KernelSize				= new ClampedIntParameter(1, 1, 20) { value = 1 };
		public VolumeParameter<bool> AnimateKernelSize		= new VolumeParameter<bool> { value = false };
		public ClampedIntParameter   MinKernelSize			= new ClampedIntParameter(1, 1, 20) { value = 1 };
		public ClampedFloatParameter SizeAnimationSpeed		= new ClampedFloatParameter(1f, 0.1f, 5f) { value = 1.0f };
		public VolumeParameter<bool> AnimateKernelOrigin	= new VolumeParameter<bool> { value = false };

		[Header("Settings for Generalized and Anisotropic:")]
		public ClampedFloatParameter Sharpness				= new ClampedFloatParameter(8f, 0.1f, 18f) { value = 8f };
		public ClampedFloatParameter Hardness				= new ClampedFloatParameter(8f, 1f, 100f) { value = 8f };
		public ClampedFloatParameter ZeroCrossing			= new ClampedFloatParameter(0.58f, 0.01f, 2f) { value = 0.58f };
		public VolumeParameter<bool> UseZeta				= new VolumeParameter<bool> { value = false };
		public ClampedFloatParameter Zeta					= new ClampedFloatParameter(1f, 0.01f, 3f) { value = 1f };

		[Header("Settings for Anisotropic:")]
		public ClampedFloatParameter Alpha = new ClampedFloatParameter(1f, 0.01f, 2f) { value = 1f };

		public KuwaharaEffectPPComponent() {}
	}
}
