using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;

namespace Caregivr
{
	public static class JsonParser
	{
		public static Procedure ParseProcedureString(string jsonString)
		{
			JObject jObject = JObject.Parse(jsonString);
			Dictionary<string, Procedure> procedureMap = ParseProcedures(jObject);
			Debug.AssertFormat(
				procedureMap.Count == 1,
				"[JsonParser] There must only be one top level procedure. Instead there is {0}.",
				procedureMap.Count
			);

			return procedureMap.Values.FirstOrDefault();
		}
		
		public static JObject LoadDependencies(JObject jProcedure)
		{
			JObject toReturn = new JObject();
			JObject dependencies = (JObject)jProcedure["dependencies"];
			if (dependencies != null)
			{
				foreach (KeyValuePair<string, JToken> depKVP in dependencies)
				{
					if (depKVP.Value.Type == JTokenType.Array)
					{
						JArray dependencyArray = (JArray)depKVP.Value;
						JObject loadedDependencies = new JObject();
						foreach (string dependencyFileName in dependencyArray)
						{
							loadedDependencies.Merge(LoadJSONFromFile("JSON/"+dependencyFileName));
						}
						toReturn.Add(depKVP.Key, loadedDependencies);
					}
				}
			}
			
			return toReturn;
		}

		public static Dictionary<string, Procedure> ParseProcedures(JObject jProcedures)
		{
			Dictionary<string, Procedure> procedureMap = new Dictionary<string, Procedure>();
			foreach (KeyValuePair<string, JToken> procedureKVP in jProcedures)
			{
				try
				{
					JObject procedureJObject = (JObject)procedureKVP.Value;
					JObject dependencies = LoadDependencies(procedureJObject);
					procedureJObject.Merge(dependencies);

					VariantDictionary procedureData = ParseProcedureData((JObject)procedureKVP.Value["data"]);
					Procedure.ProcedureType procedureType = ParseProcedureType((string)procedureKVP.Value["type"]);
					Procedure procedure;
					switch (procedureType)
					{
						case Procedure.ProcedureType.Branching:
						{
							JObject subProceduresJson = (JObject)procedureKVP.Value["procedures"];
							if (subProceduresJson == null) {
								throw new System.Exception("[JsonParser] ParseProcedures branching procedure " + procedureKVP.Key + " has no sub procedures.");
							}
							Dictionary<string, Procedure> subProcedures = ParseProcedures(subProceduresJson);
							Dictionary<Procedure, Procedure> branchMap = new Dictionary<Procedure, Procedure>();
							JObject branchesMap = (JObject)procedureKVP.Value["branches"];
							foreach (KeyValuePair<string, JToken> branchesKVP in branchesMap)
							{
								string branchStartProcedureId = branchesKVP.Key;
								string branchProcedureId = (string)branchesKVP.Value;
								if (!subProcedures.ContainsKey(branchStartProcedureId))
								{
									throw new System.Exception("[JsonParser] ParseProcedures branching procedure " + procedureKVP.Key + " does not contain branch start procedure with identifier " + branchStartProcedureId + " in its sub-procedures.");
								}
								if (!subProcedures.ContainsKey(branchProcedureId))
								{
									throw new System.Exception("[JsonParser] ParseProcedures branching procedure " + procedureKVP.Key + " does not contain branch with identifier " + branchProcedureId + " in its sub-procedures.");
								}
								branchMap.Add(subProcedures[branchStartProcedureId], subProcedures[branchProcedureId]);
							}
							procedure = new BranchingProcedure(
								procedureKVP.Key,
								(string)procedureKVP.Value["name"],
								(string)procedureKVP.Value["description"],
								procedureType,
								procedureData,
								branchMap
							);
							break;
						}
						case Procedure.ProcedureType.Sequential:
						{
							JObject subProcedures = (JObject)procedureKVP.Value["procedures"];
							if (subProcedures == null) {
								throw new System.Exception("[JsonParser] ParseProcedures sequential procedure " + procedureKVP.Key + " has no sub procedures.");
							}
							Dictionary<string, Procedure> procedureStepMap = ParseProcedures(subProcedures);
							JArray jStepIds = (JArray)procedureKVP.Value["steps"];
							List<Procedure> procedureSequentialSteps = new List<Procedure>();
							foreach (string stepId in jStepIds)
							{
								if (!procedureStepMap.ContainsKey(stepId))
								{
									throw new System.Exception("[JsonParser] ParseProcedures sequential procedure " + procedureKVP.Key + " does not contain " + stepId + " in its subprocedures.");
								}
								procedureSequentialSteps.Add(procedureStepMap[stepId]);
							}
							procedure = new SequentialProcedure(
								procedureKVP.Key,
								(string)procedureKVP.Value["name"],
								(string)procedureKVP.Value["description"],
								procedureType,
								procedureData,
								procedureSequentialSteps.ToArray()
							);
							break;
						}
						case Procedure.ProcedureType.Timer:
						{
							procedure = new TimerProcedure(
								procedureKVP.Key,
								(string)procedureKVP.Value["name"],
								(string)procedureKVP.Value["description"],
								procedureType,
								procedureData
							);
							break;
						}
						case Procedure.ProcedureType.Atomic:
						{
							procedure = new AtomicProcedure(
								procedureKVP.Key,
								(string)procedureKVP.Value["name"],
								(string)procedureKVP.Value["description"],
								procedureType,
								procedureData,
								(string)procedureKVP.Value["object_identifier"],
								(string)procedureKVP.Value["completion_event"]
							);
							break;
						}
						default:
							throw new System.Exception("[JsonParser] unknown procedure type: " + procedureType);
					}
					procedureMap.Add(procedureKVP.Key, procedure);
				}
				catch (System.Exception e)
				{
					Debug.LogException(e);
					continue;
				}
			}

			return procedureMap;
		}

		public static VariantDictionary ParseProcedureData(JObject jData)
		{
			VariantDictionary procedureData = new VariantDictionary();
			if (jData != null)
			{
				foreach (KeyValuePair<string, JToken> jDataKVP in jData)
				{
					switch (jDataKVP.Value.Type)
					{
						case JTokenType.String:
							procedureData.Add(jDataKVP.Key, new Variant(jDataKVP.Value.ToString()));
							break;
						case JTokenType.Object:
							procedureData.Add(jDataKVP.Key, new Variant(ParseProcedureData((JObject)jDataKVP.Value)));
							break;
						case JTokenType.Boolean:
							bool asBool = (bool) jDataKVP.Value;
							procedureData.Add(jDataKVP.Key, new Variant(asBool));
							break;
						case JTokenType.Float:
							float asFloat = (float) jDataKVP.Value;
							procedureData.Add(jDataKVP.Key, new Variant(asFloat));
							break;
						default:
							Debug.AssertFormat(false, "[JsonParser] ParseProcedureData() data value type not supported {0} -> {1}", jDataKVP.Key, jDataKVP.Value);
							break;
					}
				}
			}

			return procedureData;
		}

		public static Procedure.ProcedureType ParseProcedureType(string procedureTypeString)
		{
			switch (procedureTypeString)
			{
				case "sequential":
					return Procedure.ProcedureType.Sequential;
				case "atomic":
					return Procedure.ProcedureType.Atomic;
				case "branching":
					return Procedure.ProcedureType.Branching;
				case "timer":
					return Procedure.ProcedureType.Timer;
			}
			
			throw new System.Exception("[JsonParser] invalid procedure type: " + procedureTypeString);
		}

		public static JObject LoadJSONFromFile(string filePath)
		{
			TextAsset textAsset = Resources.Load<TextAsset>(filePath);
			if (textAsset == null)
			{
				throw new System.Exception("[JsonParser] LoadJSONFromFile() could not load TextAsset from path: " + filePath);
			}
			
			return JObject.Parse(textAsset.text);
		}
	}
}
