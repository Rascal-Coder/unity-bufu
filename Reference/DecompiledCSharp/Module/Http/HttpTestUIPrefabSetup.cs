using UnityEngine;

namespace Module.Http;

public class HttpTestUIPrefabSetup : MonoBehaviour
{
	[Header("按钮预制体设置说明")]
	[TextArea(5, 10)]
	public string setupInstructions = "\r\n=== 按钮预制体设置步骤 ===\r\n\r\n1. 在 Unity 中创建一个 UI > Button\r\n2. 添加 TextMeshPro - Text (UGUI) 子组件\r\n3. 将该 TextMeshPro 组件重命名为 'text'\r\n4. 设置按钮的宽度和高度（建议：宽度 200，高度 50）\r\n5. 保存为预制体\r\n\r\n预制体结构应该是：\r\n- Button (GameObject)\r\n  └── Text (TextMeshProUGUI) - name: 'text'\r\n\r\n在场景中创建测试UI：\r\n1. 创建一个空 GameObject，添加 HttpTestUI 组件\r\n2. 创建或指定一个容器（Vertical Layout Group）\r\n3. 创建或指定按钮预制体\r\n4. （可选）添加 ScrollRect 组件用于滚动\r\n5. 在 Inspector 中设置引用\r\n\r\n示例代码：\r\n```csharp\r\n// 获取测试UI组件\r\nvar httpTestUI = gameObject.GetComponent<HttpTestUI>();\r\n\r\n// 修改测试用的 OpenId\r\nhttpTestUI.testOpenId = \"your_test_openid\";\r\n```\r\n";
}
