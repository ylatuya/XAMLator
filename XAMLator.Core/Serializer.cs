using System;
using Newtonsoft.Json;

namespace XAMLator
{
	public static class Serializer
	{
		static JsonSerializerSettings JsonSettings
		{
			get
			{
				JsonSerializerSettings settings = new JsonSerializerSettings
				{
					Formatting = Formatting.None,
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					TypeNameHandling = TypeNameHandling.None,
					ObjectCreationHandling = ObjectCreationHandling.Replace,
					MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
				};
				return settings;
			}
		}

		/// <summary>
		/// Deserializes a json string into an object.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="json">Json.</param>
		public static object DeserializeJson(string json)
		{
			return DeserializeJson(json, null);
		}

		/// <summary>
		/// Deserializes a json string into an object.
		/// </summary>
		/// <returns>The object.</returns>
		/// <param name="json">Json.</param>
		/// <typeparam name="TType">The type of the object.</typeparam>
		public static TType DeserializeJson<TType>(string json)
		{
			return (TType)DeserializeJson(json, typeof(TType));
		}

		/// <summary>
		/// Serializes an object to a json string.
		/// </summary>
		/// <returns>The json string.</returns>
		/// <param name="obj">Object to serialize.</param>
		/// <typeparam name="TType">The type of the object.</typeparam>
		public static string SerializeJson<TType>(TType obj)
		{
			JsonSerializerSettings settings = JsonSettings;
			var json = JsonConvert.SerializeObject(obj, settings);
			return json;
		}

		static object DeserializeJson(string jsonString, Type type = null)
		{
			JsonSerializerSettings settings = JsonSettings;
			if (type != null)
			{
				return JsonConvert.DeserializeObject(jsonString, type, settings);
			}
			return JsonConvert.DeserializeObject(jsonString, settings);
		}
	}
}
