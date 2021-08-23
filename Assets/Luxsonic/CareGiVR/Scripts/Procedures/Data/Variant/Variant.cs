using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Caregivr
{
	// Variant
	// Variant is a holder for data that we dont know what type it will be.
	// Use the type field to determine what type the Variant is holding.
	public struct Variant
	{
		public enum VariantType
		{
			String,
			Dictionary,
			Long,
			Bool,
			Float,
		}

		public VariantType type { get; private set; }

		private string _str;
		private VariantDictionary _data;
		private long _long;
		private bool _bool;
		private float _float;
		
		public bool AsBool()
		{
			Debug.AssertFormat(
				type.Equals(VariantType.Bool),
				"[Variant] AsBool() this is not a bool."
			);
			return _bool;
		}

		public long AsLong()
		{
			Debug.AssertFormat(
				type.Equals(VariantType.Long),
				"[Variant] AsLong() this is not a long."
			);
			return _long;
		}

		public float AsFloat()
		{
			Debug.AssertFormat(
				type.Equals(VariantType.Float),
				"[Variant] AsFloat() this is not a float."
			);
			return _float;
		}

		public string AsString()
		{
			Debug.AssertFormat(
				type.Equals(VariantType.String),
				"[Variant] AsString() this is not a string."
			);
			return _str;
		}

		public VariantDictionary AsData()
		{
			Debug.AssertFormat(
				type.Equals(VariantType.Dictionary),
				"[Variant] AsData() this is not a dictionary."
			);
			return _data;
		}

		private Variant(VariantType t)
		{
			type = t;
			_bool = false;
			_str = "";
			_data = new VariantDictionary();
			_long = 0L;
			_float = 0.0f;
		}

		public Variant(bool b) : this(VariantType.Bool)
		{
			_bool = b;
		}

		public Variant(string s) : this(VariantType.String)
		{
			_str = s;
		}

		public Variant(VariantDictionary data) : this(VariantType.Dictionary)
		{
			_data = data;
		}

		public Variant(long l) : this(VariantType.Long)
		{
			_long = l;
		}
		public Variant(float f) : this(VariantType.Float)
		{
			_float = f;
		}
	}
}
