using UnityEngine.Rendering.Universal;

namespace KuwaharaURP {
	[System.Serializable]
	public class KuwaharaEffectRenderFeature : ScriptableRendererFeature {	
		public RenderPassEvent RenderOrder = RenderPassEvent.BeforeRenderingPostProcessing;

		KuwaharaEffectPass _pass;

		public override void Create() {
			_pass = new KuwaharaEffectPass();
			_pass.renderPassEvent = RenderOrder;
		}

		public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
			_pass.renderPassEvent = RenderOrder;
			renderer.EnqueuePass(_pass);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
			_pass.DeInit();
		}
	}
}
