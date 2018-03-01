//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: A package of items that can interact with the hands and be returned
//
//=============================================================================

using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class ItemPackage : MonoBehaviour
	{
		public enum ItemPackageType { Unrestricted, OneHanded, TwoHanded }

		public new string name;
		public ItemPackageType packageType = ItemPackageType.Unrestricted;
		public UnityEngine.GameObject itemPrefab; // object to be spawned on tracked controller
		public UnityEngine.GameObject otherHandItemPrefab; // object to be spawned in Other Hand
		public UnityEngine.GameObject previewPrefab; // used to preview inputObject
		public UnityEngine.GameObject fadedPreviewPrefab; // used to preview insubstantial inputObject
	}
}
