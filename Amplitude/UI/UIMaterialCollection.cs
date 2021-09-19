using System;
using System.Collections.Generic;
using Amplitude.Graphics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Amplitude.UI
{
	[CreateAssetMenu(menuName = "Amplitude/UI/Material Collection")]
	public class UIMaterialCollection : ScriptableObject, IHasLoadingStatus
	{
		public struct MaterialCollectionCollisionFreeSet
		{
			public ResolvedMaterialEntry[] CollisionFreeSet;

			public int CollisionFreeSetSize;

			private UIMaterialCollection materialCollection;

			public MaterialCollectionCollisionFreeSet(UIMaterialCollection materialCollection)
			{
				this.materialCollection = materialCollection;
				CollisionFreeSetSize = materialCollection.CollisionFreeSetSize;
				CollisionFreeSet = new ResolvedMaterialEntry[materialCollection.CollisionFreeSetSize];
				materialCollection.FillResolvedMaterialCollisionFreeSet(CollisionFreeSet);
			}

			public static uint GetIndex(UIPrimitiveType primitiveType, UIBlendType blendType, StaticString name, StaticString variant0, StaticString variant1, uint setSize)
			{
				int num = (int)primitiveType | ((int)blendType << 3);
				int num2 = (17 + num) * 23;
				num2 = ((name.Handle != 0) ? (num2 * 23 + name.Handle) : num2);
				num2 = ((variant0.Handle != 0) ? (num2 * 23 + variant0.Handle) : num2);
				num2 = ((variant1.Handle != 0) ? (num2 * 23 + variant1.Handle) : num2);
				return (uint)num2 % setSize;
			}

			public void Clear()
			{
				HashSet<Material> hashSet = new HashSet<Material>();
				int num = ((CollisionFreeSet != null) ? CollisionFreeSet.Length : 0);
				for (int i = 0; i < num; i++)
				{
					if (CollisionFreeSet[i].Material != null)
					{
						hashSet.Add(CollisionFreeSet[i].Material);
					}
					CollisionFreeSet[i] = default(ResolvedMaterialEntry);
				}
				CollisionFreeSet = null;
				foreach (Material item in hashSet)
				{
					UnityEngine.Object.DestroyImmediate(item);
				}
				hashSet.Clear();
				hashSet = null;
			}
		}

		public struct ResolvedMaterialEntry
		{
			public Material Material;

			public MaterialPropertyFieldInfo[] MaterialPropertiesInfo;
		}

		[Serializable]
		private struct UIMaterial
		{
			public UIPrimitiveType Type;

			public string Name;

			[FormerlySerializedAs("Variant")]
			public string Variant0;

			public string Variant1;

			public Material Material;

			public MaterialPropertyFieldInfo[] MaterialPropertiesInfo;

			public UIMaterial(UIPrimitiveType type, string name, string variant0, string variant1, Material material, Material originalMaterial, MaterialPropertyFieldInfo[] materialPropertiesInfo)
			{
				Type = type;
				Name = name;
				Variant0 = variant0;
				Variant1 = variant1;
				Material = material;
				MaterialPropertiesInfo = materialPropertiesInfo;
			}
		}

		public static readonly StaticString DefaultMaterialNameId = new StaticString("Default");

		private static readonly UIBlendType[] BlendTypes = Enum.GetValues(typeof(UIBlendType)) as UIBlendType[];

		private static MaterialCollectionCollisionFreeSet global;

		[SerializeField]
		private int minCollisionFreeSetSize = 512;

		[SerializeField]
		private int maxCollisionFreeSetSize = 2048;

		[SerializeField]
		private List<UIMaterial> materials = new List<UIMaterial>();

		[SerializeField]
		private Shader copyTextureShader;

		[SerializeField]
		private UIMaterialCollection fallback;

		[NonSerialized]
		private bool loaded;

		[NonSerialized]
		private int collisionFreeSetSize;

		public bool Loaded => loaded;

		public int CollisionFreeSetSize => collisionFreeSetSize;

		public LoadingStatusEnum LoadingStatus
		{
			get
			{
				if (!loaded)
				{
					return LoadingStatusEnum.NotLoaded;
				}
				return LoadingStatusEnum.Loaded;
			}
		}

		public static Material ResolveMaterial(UIPrimitiveType primitiveType, UIBlendType blendType)
		{
			int num = (int)primitiveType | ((int)blendType << 3);
			uint num2 = (uint)((17 + num) * 23) % (uint)global.CollisionFreeSetSize;
			return global.CollisionFreeSet[num2].Material;
		}

		public static Material ResolveMaterial(UIPrimitiveType primitiveType, UIBlendType blendType, StaticString name)
		{
			int num = (int)primitiveType | ((int)blendType << 3);
			int num2 = (17 + num) * 23;
			num2 = ((name.Handle != 0) ? (num2 * 23 + name.Handle) : num2);
			uint num3 = (uint)num2 % (uint)global.CollisionFreeSetSize;
			return global.CollisionFreeSet[num3].Material;
		}

		public static Material ResolveMaterial(UIPrimitiveType primitiveType, UIBlendType blendType, StaticString name, out MaterialPropertyFieldInfo[] materialPropertiesInfo)
		{
			int num = (int)primitiveType | ((int)blendType << 3);
			int num2 = (17 + num) * 23;
			num2 = ((name.Handle != 0) ? (num2 * 23 + name.Handle) : num2);
			uint num3 = (uint)num2 % (uint)global.CollisionFreeSetSize;
			Material material = global.CollisionFreeSet[num3].Material;
			materialPropertiesInfo = global.CollisionFreeSet[num3].MaterialPropertiesInfo;
			return material;
		}

		public static Material ResolveMaterial(UIPrimitiveType primitiveType, UIBlendType blendType, StaticString name, StaticString variant)
		{
			int num = (int)primitiveType | ((int)blendType << 3);
			int num2 = (17 + num) * 23;
			num2 = ((name.Handle != 0) ? (num2 * 23 + name.Handle) : num2);
			num2 = ((variant.Handle != 0) ? (num2 * 23 + variant.Handle) : num2);
			uint num3 = (uint)num2 % (uint)global.CollisionFreeSetSize;
			return global.CollisionFreeSet[num3].Material;
		}

		public static Material ResolveMaterial(UIPrimitiveType primitiveType, UIBlendType blendType, StaticString name, StaticString variant, out MaterialPropertyFieldInfo[] materialPropertiesInfo)
		{
			int num = (int)primitiveType | ((int)blendType << 3);
			int num2 = (17 + num) * 23;
			num2 = ((name.Handle != 0) ? (num2 * 23 + name.Handle) : num2);
			num2 = ((variant.Handle != 0) ? (num2 * 23 + variant.Handle) : num2);
			uint num3 = (uint)num2 % (uint)global.CollisionFreeSetSize;
			Material material = global.CollisionFreeSet[num3].Material;
			materialPropertiesInfo = global.CollisionFreeSet[num3].MaterialPropertiesInfo;
			return material;
		}

		public static Material ResolveMaterial(UIPrimitiveType primitiveType, UIBlendType blendType, StaticString name, StaticString variant0, StaticString variant1)
		{
			int num = (int)primitiveType | ((int)blendType << 3);
			int num2 = (17 + num) * 23;
			num2 = ((name.Handle != 0) ? (num2 * 23 + name.Handle) : num2);
			num2 = ((variant0.Handle != 0) ? (num2 * 23 + variant0.Handle) : num2);
			num2 = ((variant1.Handle != 0) ? (num2 * 23 + variant1.Handle) : num2);
			uint num3 = (uint)num2 % (uint)global.CollisionFreeSetSize;
			return global.CollisionFreeSet[num3].Material;
		}

		public static Material ResolveMaterial(UIPrimitiveType primitiveType, UIBlendType blendType, StaticString name, StaticString variant0, StaticString variant1, out MaterialPropertyFieldInfo[] materialPropertiesInfo)
		{
			int num = (int)primitiveType | ((int)blendType << 3);
			int num2 = (17 + num) * 23;
			num2 = ((name.Handle != 0) ? (num2 * 23 + name.Handle) : num2);
			num2 = ((variant0.Handle != 0) ? (num2 * 23 + variant0.Handle) : num2);
			num2 = ((variant1.Handle != 0) ? (num2 * 23 + variant1.Handle) : num2);
			uint num3 = (uint)num2 % (uint)global.CollisionFreeSetSize;
			Material material = global.CollisionFreeSet[num3].Material;
			materialPropertiesInfo = global.CollisionFreeSet[num3].MaterialPropertiesInfo;
			return material;
		}

		public Shader GetCopyTextureShader()
		{
			if (copyTextureShader != null)
			{
				return copyTextureShader;
			}
			if (fallback != null)
			{
				return fallback.GetCopyTextureShader();
			}
			return null;
		}

		public void LoadIfNecessary()
		{
			if (!loaded)
			{
				Load();
			}
		}

		public void UnloadIfNecessary()
		{
			if (loaded)
			{
				Unload();
			}
		}

		private static void SetupMaterialBlendType(Material material, UIBlendType blendType)
		{
			BlendMode value;
			BlendMode value2;
			switch (blendType)
			{
			case UIBlendType.Standard:
				value = BlendMode.One;
				value2 = BlendMode.OneMinusSrcAlpha;
				break;
			case UIBlendType.Additive:
				value = BlendMode.One;
				value2 = BlendMode.One;
				break;
			case UIBlendType.SoftAdditive:
				value = BlendMode.OneMinusDstColor;
				value2 = BlendMode.One;
				break;
			case UIBlendType.Multiplicative:
				value = BlendMode.DstColor;
				value2 = BlendMode.Zero;
				break;
			default:
				throw new NotImplementedException();
			}
			material.SetInt("_SrcBlend", (int)value);
			material.SetInt("_DestBlend", (int)value2);
		}

		private void Load()
		{
			HashSet<uint> collisionDetectionSet = null;
			collisionFreeSetSize = FindCollisionFreeSetSize(ref collisionDetectionSet);
			Diagnostics.Log("In {0} ({1}) using a {2} collisionFreeHashSetSize", base.name, GetType(), collisionFreeSetSize);
			loaded = true;
			global = new MaterialCollectionCollisionFreeSet(this);
		}

		private int FindCollisionFreeSetSize(ref HashSet<uint> collisionDetectionSet)
		{
			collisionDetectionSet = collisionDetectionSet ?? new HashSet<uint>();
			for (int i = minCollisionFreeSetSize; i < maxCollisionFreeSetSize; i++)
			{
				collisionDetectionSet.Clear();
				if (!HasCollision(i, ref collisionDetectionSet))
				{
					return i;
				}
			}
			collisionDetectionSet.Clear();
			Diagnostics.LogError($"In {GetType()} {base.name} unable to find a collision free set size. maxCollisionFreeSetSize = {maxCollisionFreeSetSize}");
			return -1;
		}

		private bool HasCollision(int collisionSetSize, ref HashSet<uint> collisionDetectionSet)
		{
			int count = materials.Count;
			for (int i = 0; i < count; i++)
			{
				UIMaterial uIMaterial = materials[i];
				for (int j = 0; j < BlendTypes.Length; j++)
				{
					StaticString staticString = new StaticString(uIMaterial.Name);
					uint item = MaterialCollectionCollisionFreeSet.GetIndex(variant0: new StaticString(uIMaterial.Variant0), variant1: new StaticString(uIMaterial.Variant1), primitiveType: uIMaterial.Type, blendType: BlendTypes[j], name: staticString, setSize: (uint)collisionSetSize);
					if (!collisionDetectionSet.Add(item))
					{
						return true;
					}
					if (staticString == DefaultMaterialNameId)
					{
						uint index = MaterialCollectionCollisionFreeSet.GetIndex(uIMaterial.Type, BlendTypes[j], StaticString.Empty, new StaticString(uIMaterial.Variant0), new StaticString(uIMaterial.Variant1), (uint)collisionSetSize);
						if (!collisionDetectionSet.Add(index))
						{
							return true;
						}
					}
				}
			}
			if ((bool)fallback && fallback.HasCollision(collisionSetSize, ref collisionDetectionSet))
			{
				return true;
			}
			return false;
		}

		private bool FillResolvedMaterialCollisionFreeSet(ResolvedMaterialEntry[] collisionFreeSet)
		{
			int setSize = collisionFreeSet.Length;
			ResolvedMaterialEntry resolvedMaterialEntry = default(ResolvedMaterialEntry);
			for (int i = 0; i < materials.Count; i++)
			{
				UIMaterial uIMaterial = materials[i];
				for (int j = 0; j < BlendTypes.Length; j++)
				{
					StaticString staticString = new StaticString(uIMaterial.Name);
					uint num = MaterialCollectionCollisionFreeSet.GetIndex(variant0: new StaticString(uIMaterial.Variant0), variant1: new StaticString(uIMaterial.Variant1), primitiveType: uIMaterial.Type, blendType: BlendTypes[j], name: staticString, setSize: (uint)setSize);
					if (collisionFreeSet[num].Material != null)
					{
						return false;
					}
					Material material = new Material(uIMaterial.Material);
					SetupMaterialBlendType(material, BlendTypes[j]);
					resolvedMaterialEntry.Material = material;
					resolvedMaterialEntry.MaterialPropertiesInfo = uIMaterial.MaterialPropertiesInfo;
					collisionFreeSet[num] = resolvedMaterialEntry;
					if (staticString == DefaultMaterialNameId)
					{
						uint index = MaterialCollectionCollisionFreeSet.GetIndex(uIMaterial.Type, BlendTypes[j], StaticString.Empty, new StaticString(uIMaterial.Variant0), new StaticString(uIMaterial.Variant1), (uint)setSize);
						if (collisionFreeSet[index].Material != null)
						{
							return false;
						}
						collisionFreeSet[index] = resolvedMaterialEntry;
					}
				}
			}
			if ((bool)fallback && !fallback.FillResolvedMaterialCollisionFreeSet(collisionFreeSet))
			{
				return false;
			}
			return true;
		}

		private void Unload()
		{
			global.Clear();
			global = default(MaterialCollectionCollisionFreeSet);
			if (fallback != null)
			{
				fallback.UnloadIfNecessary();
			}
			collisionFreeSetSize = 0;
			loaded = false;
		}
	}
}
