/*
This is free software distributed under the terms of the MIT license, reproduced below. It may be used for any purpose, including commercial purposes, at absolutely no cost. No paperwork, no royalties, no GNU-like "copyleft" restrictions. Just download and enjoy.

Copyright (c) 2014 Chimera Software, LLC

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using TMPro;



public class PUTMProInputField : PUTMPro {

	public string onValueChanged;
	public string placeholder;
	public int? characterLimit;
	public char? asteriskChar;
	public PlanetUnity2.InputFieldContentType? contentType;
	public PlanetUnity2.InputFieldLineType? lineType;
	public Color? selectionColor;


	public TMP_InputField field;
	public PUTMPro placeholderText;
	public PUGameObject inputField;
	public PUGameObject text;
	public PUGameObject textArea;

	private string regexValidation = null;

	public string GetValue() {
		if (field.text.Length > 0) {
			return field.text;
		}
		if (placeholderText != null && placeholderText.textGUI.text.Length > 0) {
			return placeholderText.textGUI.text;
		}
		return "";
	}

	public override void gaxb_final(TB.TBXMLElement element, object _parent, Hashtable args) {
		base.gaxb_final (element, _parent, args);

		string attr;

		if (element != null) {
			attr = element.GetAttribute ("regexValidation");
			if (attr != null) {
				regexValidation = attr;
			}

			attr = element.GetAttribute("onValueChanged");
			if(attr != null) { attr = PlanetUnityOverride.processString(_parent, attr); }
			if(attr != null) { onValueChanged = (attr); } 

			attr = element.GetAttribute("placeholder");
			if(attr != null) { attr = PlanetUnityOverride.processString(_parent, attr); }
			if(attr != null) { placeholder = (attr); } 

			attr = element.GetAttribute("characterLimit");
			if(attr != null) { attr = PlanetUnityOverride.processString(_parent, attr); }
			if(attr != null) { characterLimit = (int)float.Parse(attr); } 

			attr = element.GetAttribute("asteriskChar");
			if(attr != null) { attr = PlanetUnityOverride.processString(_parent, attr); }
			if(attr != null) { asteriskChar = char.Parse(attr); } 

			attr = element.GetAttribute("contentType");
			if(attr != null) { attr = PlanetUnityOverride.processString(_parent, attr); }
			if(attr != null) { contentType = (PlanetUnity2.InputFieldContentType)System.Enum.Parse(typeof(PlanetUnity2.InputFieldContentType), attr); } 

			attr = element.GetAttribute("lineType");
			if(attr != null) { attr = PlanetUnityOverride.processString(_parent, attr); }
			if(attr != null) { lineType = (PlanetUnity2.InputFieldLineType)System.Enum.Parse(typeof(PlanetUnity2.InputFieldLineType), attr); } 

			attr = element.GetAttribute("selectionColor");
			if(attr != null) { attr = PlanetUnityOverride.processString(_parent, attr); }
			if(attr != null) { selectionColor = new Color().PUParse(attr); } 

		}
	}

	// This is required for application-level subclasses
	public override void gaxb_init ()
	{
		base.gaxb_init ();
		gaxb_addToParent();
	}

	public override void gaxb_complete ()
	{
		base.gaxb_complete ();

		// At this point, our gameObject is our "text" game object.  We want to maneuver things around
		// until its like:
		// --> TMPro - Input Field
		//   --> Text Area
		//      --> Placeholder
		//      --> Text


		// 0) first, swap out our TMPro text and replace our gameObject with one with the text field
		GameObject textGameObject = gameObject;

		// Next, we create a new gameObject, and put the Text-created gameObject inside me
		gameObject = new GameObject ("<TMP_InputField/>", typeof(RectTransform));
		gameObject.transform.SetParent (textGameObject.transform.parent, false);
		UpdateRectTransform ();


		// 0.5) Create a game object for the field
		inputField = new PUGameObject();
		inputField.title = "TMP_InputField";
		inputField.SetFrame (0, 0, 0, 0, 0, 0, "stretch,stretch");
		inputField.LoadIntoPUGameObject (this);

		// 1) create the hierarchy of stuff under me
		textArea = new PUGameObject();
		textArea.title = "Text Area";
		textArea.SetFrame (0, 0, 0, 0, 0, 0, "stretch,stretch");
		textArea.LoadIntoPUGameObject (inputField);

		// 2) placeholder
		if (placeholder != null) {
			placeholderText = new PUTMPro ();
			placeholderText.title = "Placeholder";
			placeholderText.value = this.placeholder;
			placeholderText.LoadIntoPUGameObject (textArea);

			placeholderText.textGUI.overflowMode = this.textGUI.overflowMode;

			placeholderText.textGUI.alignment = this.textGUI.alignment;
			placeholderText.textGUI.font = this.textGUI.font;
			placeholderText.textGUI.fontSize = this.textGUI.fontSize;
			placeholderText.textGUI.fontStyle = this.textGUI.fontStyle;
			placeholderText.textGUI.color = this.textGUI.color - new Color(0,0,0,0.5f);
			placeholderText.textGUI.lineSpacing = this.textGUI.lineSpacing;

			placeholderText.gameObject.FillParentUI ();
		}

		// 3) text
		text = new PUGameObject();
		text.SetFrame (0, 0, 0, 0, 0, 0, "stretch,stretch");
		text.LoadIntoPUGameObject (textArea);

		GameObject.Destroy (text.gameObject);
		text.gameObject = textGameObject;

		// Move the text to be the child of the input field
		textGameObject.name = "Text";
		textGameObject.transform.SetParent (textArea.rectTransform, false);
		textGameObject.FillParentUI ();
		(textGameObject.transform as RectTransform).pivot = Vector2.zero;


		// now that we have the hierarchy, fille out the input field
		field = inputField.gameObject.AddComponent<TMP_InputField> ();

		field.transition = Selectable.Transition.None;

		field.targetGraphic = inputField.gameObject.AddComponent<InvisibleHitGraphic> ();
		field.textViewport = textArea.rectTransform;
		field.textComponent = textGUI;

		if (asteriskChar != null) {
			field.asteriskChar = asteriskChar.Value;
		}

		if (contentType == PlanetUnity2.InputFieldContentType.standard) {
			field.contentType = TMP_InputField.ContentType.Standard;
		} else if (contentType == PlanetUnity2.InputFieldContentType.autocorrected) {
			field.contentType = TMP_InputField.ContentType.Autocorrected;
		} else if (contentType == PlanetUnity2.InputFieldContentType.integer) {
			field.contentType = TMP_InputField.ContentType.IntegerNumber;
		} else if (contentType == PlanetUnity2.InputFieldContentType.number) {
			field.contentType = TMP_InputField.ContentType.DecimalNumber;
		} else if (contentType == PlanetUnity2.InputFieldContentType.alphanumeric) {
			field.contentType = TMP_InputField.ContentType.Alphanumeric;
		} else if (contentType == PlanetUnity2.InputFieldContentType.name) {
			field.contentType = TMP_InputField.ContentType.Name;
		} else if (contentType == PlanetUnity2.InputFieldContentType.email) {
			field.contentType = TMP_InputField.ContentType.EmailAddress;
		} else if (contentType == PlanetUnity2.InputFieldContentType.password) {
			field.contentType = TMP_InputField.ContentType.Password;
			field.inputType = TMP_InputField.InputType.Password;
		} else if (contentType == PlanetUnity2.InputFieldContentType.pin) {
			field.contentType = TMP_InputField.ContentType.Pin;
		} else if (contentType == PlanetUnity2.InputFieldContentType.custom) {
			field.contentType = TMP_InputField.ContentType.Custom;

			field.onValidateInput += ValidateInput;
		}

		if (lineType == PlanetUnity2.InputFieldLineType.single) {
			field.lineType = TMP_InputField.LineType.SingleLine;
		} else if (lineType == PlanetUnity2.InputFieldLineType.multiSubmit) {
			field.lineType = TMP_InputField.LineType.MultiLineSubmit;
		} else if (lineType == PlanetUnity2.InputFieldLineType.multiNewline) {
			field.lineType = TMP_InputField.LineType.MultiLineNewline;
		}

		if (characterLimit != null) {
			field.characterLimit = (int)characterLimit;
		}

		if (selectionColor != null) {
			field.selectionColor = selectionColor.Value;
		}

		// This is probably not the best way to do this, but 4.60.f1 removed the onSubmit event
		field.onEndEdit.AddListener ((value) => {
			if(onValueChanged != null){
				NotificationCenter.postNotification (Scope (), this.onValueChanged, NotificationCenter.Args("sender", this));
			}
		});

		foreach (Object obj in gameObject.GetComponentsInChildren<DetectTextClickTMPro>()) {
			GameObject.Destroy (obj);
		}

		if (this.value == null) {
			this.value = "";
		}

		field.text = this.value;

		if (placeholder != null) {
			field.placeholder = placeholderText.textGUI;
		}
	}

	// only allow what FanDuel server supports
	private char ValidateInput(string text, int charIndex, char addedChar)
	{
		// if we don't have a regex then we'll allow any char
		// if we do then check the added char against the specified regex
		if (regexValidation == null || Regex.IsMatch(""+addedChar, regexValidation))
			return addedChar;

		return '\0';
	}

}
