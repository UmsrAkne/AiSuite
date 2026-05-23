using System.Text.Json;
using System.Text.Json.Nodes;

namespace AiSuite.Utils
{
    public class JsonUtil
    {
        /// <summary>
        /// JSON要素内を再帰的に探索し、指定された title を持つノード（オブジェクト）を返します。
        /// </summary>
        public static JsonElement? FindNodeByTitle(JsonElement element, string targetTitle)
        {
            // 1. 現在の要素が「オブジェクト（連想配列）」の場合
            if (element.ValueKind == JsonValueKind.Object)
            {
                // title プロパティを持ち、かつ値が目的のノード名と一致するかチェック
                if (element.TryGetProperty("title", out var titleElement)
                    && titleElement.ValueKind == JsonValueKind.String
                    && titleElement.GetString() == targetTitle)
                {
                    return element; // 条件に一致したオブジェクトを返す
                }

                // 一致しない場合は、そのオブジェクトの配下にあるすべてのプロパティ（子階層）をループ探索
                foreach (var property in element.EnumerateObject())
                {
                    var result = FindNodeByTitle(property.Value, targetTitle);
                    if (result != null)
                    {
                        return result; // 見つかったら上の階層へ引き継ぐ
                    }
                }
            }

            // 2. 現在の要素が「配列（リスト）」の場合
            else if (element.ValueKind == JsonValueKind.Array)
            {
                // 配列の中身（各要素）を一つずつ探索
                foreach (var item in element.EnumerateArray())
                {
                    var result = FindNodeByTitle(item, targetTitle);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            // 見つからなかった場合は null を返す
            return null;
        }

        /// <summary>
        /// 指定されたタイトルを持つノードを再帰的に探し、widgets_valuesの最初の要素を書き換えます。
        /// </summary>
        /// <param name="node">探索対象のJsonNode</param>
        /// <param name="targetTitle">探したいノードのtitle</param>
        /// <param name="newValue">書き換える新しい文字列</param>
        /// <returns>書き換えに成功した場合は true、見つからなかった場合は false</returns>
        public static bool UpdateWidgetsValueByTitle(JsonNode node, string targetTitle, string newValue)
        {
            if (node == null)
            {
                return false;
            }

            // 1. 現在の要素が「オブジェクト（連想配列）」の場合
            if (node is JsonObject jsonObject)
            {
                // title プロパティが存在し、値が目的のタイトルと一致するかチェック
                if (jsonObject.TryGetPropertyValue("title", out var titleNode)
                    && titleNode?.ToString() == targetTitle)
                {
                    // widgets_values プロパティが存在し、それが配列（JsonArray）かチェック
                    if (jsonObject.TryGetPropertyValue("widgets_values", out var widgetsNode)
                        && widgetsNode is JsonArray widgetsArray)
                    {
                        if (widgetsArray.Count > 0)
                        {
                            // 最初の要素（インデックス0）を新しい値に書き換える
                            widgetsArray[0] = JsonValue.Create(newValue);
                            return true; // 書き換え成功
                        }
                    }
                }

                // 一致しない、または書き換えに失敗した場合は、子プロパティをループして再帰探索
                foreach (var property in jsonObject)
                {
                    if (UpdateWidgetsValueByTitle(property.Value, targetTitle, newValue))
                    {
                        return true; // 子階層で書き換えに成功したら終了
                    }
                }
            }

            // 2. 現在の要素が「配列」の場合
            else if (node is JsonArray jsonArray)
            {
                // 配列内の各要素をループして再帰探索
                foreach (var item in jsonArray)
                {
                    if (UpdateWidgetsValueByTitle(item, targetTitle, newValue))
                    {
                        return true; // 配列の要素内で書き換えに成功したら終了
                    }
                }
            }

            return false;
        }
    }
}