/*
This is free software distributed under the terms of the MIT license, reproduced below. It may be used for any purpose, including commercial purposes, at absolutely no cost. No paperwork, no royalties, no GNU-like "copyleft" restrictions. Just download and enjoy.

Copyright (c) 2014 Chimera Software, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using System.Xml;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System;

public class PUTMProFast : PUTMPro {

	public PUTMProFast() {

	}

	public override void gaxb_final(XmlReader reader, object _parent, Hashtable args) {
		base.gaxb_final(reader, _parent, args);
	}

	public override void GenerateTextComponent() {
		GameObject childObject = new GameObject ("TMPro");
		childObject.transform.SetParent (rectTransform, false);

		TextContainer container = childObject.AddComponent<TextContainer> ();
		container.width = rectTransform.rect.width;
		container.height = rectTransform.rect.height;
		container.anchorPosition = TextContainerAnchors.BottomLeft;

		text = childObject.AddComponent<TextMeshPro> ();
		text.font = GetFont(font);
		text.enableWordWrapping = enableWordWrapping;
		text.richText = true;
		if(fontStyle != null){
			text.fontStyle = (FontStyles)Enum.Parse (typeof(FontStyles), fontStyle);
		}
		text.isOverlay = true;
		text.isOrthographic = true;
		text.fontSize = fontSize;
		text.OverflowMode = TextOverflowModes.Ellipsis;
		text.extraPadding = true;

		if (maxVisibleLines > 0) {
			textGUI.maxVisibleLines = maxVisibleLines;
		}
		text.enableAutoSizing = sizeToFit;
		text.fontSizeMin = minSize;
		text.fontSizeMax = maxSize;

		//public enum TextAlignmentOptions { TopLeft = 0, Top = 1, TopRight = 2, TopJustified = 3, Left = 4, Center = 5, Right = 6, Justified = 7, BottomLeft = 8, Bottom = 9, BottomRight = 10, BottomJustified = 11, BaselineLeft = 12, Baseline = 13, BaselineRight = 14, BaselineJustified = 15 };
		text.alignment = alignment;

		text.color = fontColor;
		text.text = PlanetUnityStyle.ReplaceStyleTags(value);

		text.ForceMeshUpdate ();

		text.fontMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.5f); // You can play with the value to get the result you want.

		NotificationCenter.addObserver(this, "RemoveAllTMProFast", null, (args, name) => {
			LeanTween.alpha(gameObject, 0, 0.66f).setEase(LeanTweenType.easeOutCubic).setDestroyOnComplete(true);
		});

	}

}
