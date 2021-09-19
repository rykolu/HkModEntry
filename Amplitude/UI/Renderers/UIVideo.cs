using System;
using System.Collections;
using System.IO;
using Amplitude.Framework;
using Amplitude.Framework.Media;
using UnityEngine;

namespace Amplitude.UI.Renderers
{
	[ExecuteInEditMode]
	public class UIVideo : UIAbstractImage
	{
		public enum FitMode
		{
			None,
			PreserveRatio,
			Stretch,
			Horizontal,
			Vertical
		}

		public enum PlayState
		{
			Playing,
			Paused,
			Stopped,
			Finished
		}

		[SerializeField]
		[Path(FileLocation.RelativeToAssetsFolder, "mkv,mp4,avi,flv,wmv,mov,webm")]
		private string path = string.Empty;

		[SerializeField]
		private FitMode fit;

		[SerializeField]
		private bool loop;

		[SerializeField]
		private bool autoPlay;

		[SerializeField]
		[UIMaterialId(UIPrimitiveType.Rect)]
		private UIMaterialId material;

		[NonSerialized]
		private PlayState state = PlayState.Stopped;

		[NonSerialized]
		private IMovieController movieController;

		[NonSerialized]
		private Amplitude.Framework.Guid renderTextureGuid = Amplitude.Framework.Guid.Null;

		[NonSerialized]
		private UnityCoroutine prepareCoroutine;

		public string Path
		{
			get
			{
				return path;
			}
			set
			{
				if (path != value)
				{
					string previous = path;
					path = value;
					OnPathChanged(previous, path);
				}
			}
		}

		public FitMode Fit
		{
			get
			{
				return fit;
			}
			set
			{
				fit = value;
			}
		}

		public bool Loop
		{
			get
			{
				return loop;
			}
			set
			{
				if (loop != value)
				{
					loop = value;
					OnLoopChanged(!loop, loop);
				}
			}
		}

		public bool AutoPlay
		{
			get
			{
				return autoPlay;
			}
			set
			{
				autoPlay = value;
			}
		}

		public float Time
		{
			get
			{
				if (movieController != null)
				{
					return movieController.Time;
				}
				return 0f;
			}
			set
			{
				if (movieController != null)
				{
					movieController.Time = value;
				}
			}
		}

		public float Duration
		{
			get
			{
				if (movieController != null)
				{
					return movieController.GetDuration();
				}
				return 0f;
			}
		}

		public bool IsFinishedPlaying => state == PlayState.Finished;

		public bool IsPaused => state == PlayState.Paused;

		public bool IsPlaying => state == PlayState.Playing;

		public PlayState State => state;

		public override UIMaterialId Material
		{
			get
			{
				return material;
			}
			set
			{
				material = value;
				dirtyness = 2;
			}
		}

		public event Action<UIVideo> ReadyToPlay;

		public event Action<UIVideo> Started;

		public event Action<UIVideo> FinishedPlaying;

		public void Play()
		{
			if (state != 0)
			{
				state = PlayState.Playing;
				PrepareRender();
				SynchronizeMovieController();
			}
		}

		public void Pause()
		{
			if (state != PlayState.Paused)
			{
				state = PlayState.Paused;
				SynchronizeMovieController();
			}
		}

		public void Stop()
		{
			if (state != PlayState.Stopped)
			{
				state = PlayState.Stopped;
				SynchronizeMovieController();
			}
		}

		internal bool TryGetInternalMovieController(out IMovieController movieController)
		{
			movieController = this.movieController;
			return movieController != null;
		}

		protected override void Load()
		{
			base.Load();
			UIServiceAccessManager.Instance.LoadIfNecessary();
			if (IsPathValid())
			{
				prepareCoroutine = UnityCoroutine.StartCoroutine(this, DoPrepareMovieController, DoPrepareExceptionHandler);
			}
		}

		protected override void Unload()
		{
			if (prepareCoroutine != null && prepareCoroutine.Coroutine != null)
			{
				StopCoroutine(prepareCoroutine.Coroutine);
				prepareCoroutine = null;
			}
			if (movieController != null)
			{
				UIRenderingManager.Instance.UnregisterTexture(movieController.Texture);
				renderTextureGuid = Amplitude.Framework.Guid.Null;
				movieController?.Dispose();
				movieController = null;
			}
			base.Unload();
		}

		protected new Rect ComputeLocalRect()
		{
			Rect localRect = UITransform.LocalRect;
			if (fit != FitMode.Stretch)
			{
				texture.RequestSize();
				Vector2 widthHeight = texture.WidthHeight;
				FitMode fitMode = fit;
				if (fitMode == FitMode.PreserveRatio)
				{
					float num = localRect.width / localRect.height;
					float aspect = texture.Aspect;
					fitMode = ((num <= aspect) ? FitMode.Horizontal : FitMode.Vertical);
				}
				switch (fitMode)
				{
				case FitMode.None:
					localRect.width = widthHeight.x;
					localRect.height = widthHeight.y;
					break;
				case FitMode.Horizontal:
				{
					float num4 = Mathf.Min(localRect.width / texture.Aspect, localRect.height);
					float num5 = (localRect.height - num4) * 0.5f;
					localRect.yMin += num5;
					localRect.height = num4;
					break;
				}
				case FitMode.Vertical:
				{
					float num2 = Mathf.Min(localRect.height * texture.Aspect, localRect.width);
					float num3 = (localRect.width - num2) * 0.5f;
					localRect.xMin += num3;
					localRect.width = num2;
					break;
				}
				}
			}
			return localRect;
		}

		protected override void Render(UIPrimitiveDrawer drawer)
		{
			PrepareRender();
			if (movieController != null && movieController.IsReadyToPlay && !renderTextureGuid.IsNull)
			{
				Rect position = ComputeLocalRect();
				AffineTransform2d coordinatesTransform = base.TextureTransformation;
				if (movieController.IsRequiringVerticalFlip)
				{
					coordinatesTransform.Scale.y *= -1f;
				}
				Color color = base.Color;
				drawer.Rect(ref UITransform.MatrixAtomId, ref position, material.Id, texture, ref coordinatesTransform, ref color, base.Gradient, blendType);
			}
		}

		private bool IsPathValid()
		{
			if (!string.IsNullOrEmpty(path))
			{
				string text = path;
				if (!System.IO.Path.IsPathRooted(path))
				{
					text = System.IO.Path.Combine(UnityEngine.Application.dataPath, path);
				}
				return File.Exists(text);
			}
			return false;
		}

		private IEnumerator DoPrepareMovieController()
		{
			yield return Services.DoWaitForBindService<IMoviePlaybackService>();
			IMoviePlaybackService service = Services.GetService<IMoviePlaybackService>();
			string text = path;
			if (!System.IO.Path.IsPathRooted(path))
			{
				text = System.IO.Path.Combine(UnityEngine.Application.dataPath, path);
			}
			movieController = service.LoadMovieFromFile(text);
			if (movieController != null)
			{
				movieController.IsLooping = loop;
				movieController.FinishedPlaying += OnMovieControllerFinishedPlaying;
				movieController.ReadyToPlay += OnMovieControllerReadyToPlay;
				movieController.Started += OnMovieControllerStarted;
			}
			else
			{
				Diagnostics.LogError("Unable to create the movie controller for playing '{0}'.", path);
			}
			prepareCoroutine = null;
		}

		private void DoPrepareExceptionHandler(object sender, CoroutineExceptionEventArgs e)
		{
			Diagnostics.LogException(e.Exception);
		}

		private void PrepareRender()
		{
			if (renderTextureGuid.IsNull && movieController != null && movieController.Texture != null)
			{
				renderTextureGuid = UIRenderingManager.Instance.RegisterTexture(movieController.Texture);
				texture = new UITexture(renderTextureGuid, UITextureFlags.AlphaStraight, UITextureColorFormat.Srgb, movieController.Texture);
				texture.RequestAsset();
			}
		}

		private void SynchronizeMovieController()
		{
			if (movieController != null)
			{
				switch (state)
				{
				case PlayState.Playing:
					movieController.Play();
					break;
				case PlayState.Paused:
					movieController.Pause();
					break;
				case PlayState.Stopped:
					movieController.Stop();
					break;
				}
			}
		}

		private void OnMovieControllerFinishedPlaying(IMovieController movieController, MoviePlaybackEvent moviePlaybackEvent)
		{
			if (!loop)
			{
				state = PlayState.Finished;
				this.FinishedPlaying?.Invoke(this);
			}
		}

		private void OnMovieControllerReadyToPlay(IMovieController movieController, MoviePlaybackEvent moviePlaybackEvent)
		{
			if (autoPlay)
			{
				state = PlayState.Playing;
			}
			PrepareRender();
			SynchronizeMovieController();
			this.ReadyToPlay?.Invoke(this);
		}

		private void OnMovieControllerStarted(IMovieController movieController, MoviePlaybackEvent moviePlaybackEvent)
		{
			this.Started?.Invoke(this);
		}

		private void OnLoopChanged(bool previous, bool next)
		{
			if (movieController != null)
			{
				movieController.IsLooping = next;
			}
		}

		private void OnPathChanged(string previous, string next)
		{
			TryReload();
		}
	}
}
