using UnityEngine;
using System.Xml;
using System.Collections;
using UnityEngine.UI;
using System;
using TMPro;

public class PUTMProCircle : PUTMPro {

	private bool RegenerateText = true;

	public float angle = 0.0f;

	public override void gaxb_final(XmlReader reader, object _parent, Hashtable args) {
		base.gaxb_final (reader, _parent, args);

		string attrib;

		if (reader != null) {
			attrib = reader.GetAttribute ("angle");
			if (attrib != null) {
				angle = float.Parse (attrib);
			}
		}
	}


	// This is required for application-level subclasses
	public override void gaxb_init ()
	{
		title = "TMProCircle";
		base.gaxb_init ();
		gaxb_addToParent();
	}

	public override void gaxb_complete() {

		base.gaxb_complete ();

		m_TextComponent = gameObject.GetComponent<TextMeshProUGUI>();

		ScheduleForUpdate ();
	}

	public override void Update() {
		UpdateTextToFitCurve ();
	}

	private Vector2 PositionForAngle(float r) {
		float width = rectTransform.rect.width * 0.415f;
		float height = rectTransform.rect.height * 0.415f;

		float mW = Mathf.Sqrt(2) * (width * 0.5f) + (width * 0.5f);
		float mH = Mathf.Sqrt(2) * (height * 0.5f) + (height * 0.5f);

		return new Vector2 (Mathf.Cos (r * Mathf.Deg2Rad) * mW, Mathf.Sin (r * Mathf.Deg2Rad) * mH);
	}


	private TextMeshProUGUI m_TextComponent;

		
	public void UpdateTextToFitCurve()
	{
		Vector3[] vertices;
		Matrix4x4 matrix;

		m_TextComponent.havePropertiesChanged = true; // Need to force the TextMeshPro Object to be updated.

		m_TextComponent.ForceMeshUpdate(); // Generate the mesh and populate the textInfo with data we can use and manipulate.

		TMP_TextInfo textInfo = m_TextComponent.textInfo;
		int characterCount = textInfo.characterCount;

		if (characterCount == 0)
			return;

		float radius = rectTransform.rect.width * 0.415f;
		float anglePerUnit = (1.0f / radius) * Mathf.Rad2Deg;
	
		for (int i = 0; i < characterCount; i++)
		{
			if (!textInfo.characterInfo[i].isVisible)
				continue;

			int vertexIndex = textInfo.characterInfo[i].vertexIndex;

			// Get the index of the mesh used by this character.
			int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

			vertices = textInfo.meshInfo[materialIndex].vertices;

			// Compute the baseline mid point for each character
			float midX = (vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2;
			Vector3 offsetToMidBaseline = new Vector2(midX, textInfo.characterInfo[i].baseLine);

			// Apply offset to adjust our pivot point.
			vertices[vertexIndex + 0] += -offsetToMidBaseline;
			vertices[vertexIndex + 1] += -offsetToMidBaseline;
			vertices[vertexIndex + 2] += -offsetToMidBaseline;
			vertices[vertexIndex + 3] += -offsetToMidBaseline;

			// find the angle we need to travel for the xadvance
			float rot = angle - (midX * anglePerUnit);
			Vector2 pos = PositionForAngle (rot);
			matrix = Matrix4x4.TRS(pos, Quaternion.Euler(0, 0, rot - 90.0f), Vector3.one);

			vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
			vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
			vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
			vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);
		}


		// Upload the mesh with the revised information
		m_TextComponent.UpdateVertexData();

	}

}
