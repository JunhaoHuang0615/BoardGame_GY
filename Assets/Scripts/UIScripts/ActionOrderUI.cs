using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionOrderUI : MonoBehaviour
{
    public GameObject unitSlotPrefab; //单位槽位的预制体（可选，如果为空则自动创建）
    public Transform contentParent; //内容父对象（用于放置单位槽位）
    public RectTransform uiPanel; //UI面板

    private List<ActionOrderSlot> slots = new List<ActionOrderSlot>();
    private GameManager gm;

    //允许外部设置contentParent
    public void SetContentParent(Transform parent)
    {
        contentParent = parent;
    }

    private void Awake()
    {
        // 尝试获取 GameManager，如果还没有初始化则稍后重试
        gm = GameManager.Instance;
    }

    private void Start()
    {
        // 延迟初始化，确保 GameManager 已经初始化
        StartCoroutine(DelayedInitializeUI());
    }

    private IEnumerator DelayedInitializeUI()
    {
        // 等待 GameManager 初始化完成
        while (gm == null)
        {
            gm = GameManager.Instance;
            if (gm == null)
            {
                yield return null; // 等待一帧后重试
            }
        }

        // 初始化UI
        InitializeUI();
    }

    private void Update()
    {
        //每帧更新UI显示
        UpdateUI();
    }

    //自动创建内容容器
    private void CreateContentParent()
    {
        //先检查是否已经存在名为 "Content" 的子对象
        Transform existingContent = transform.Find("Content");
        if (existingContent != null)
        {
            contentParent = existingContent;
            Debug.Log("ActionOrderUI: 发现已存在的 Content 对象，使用它作为 contentParent");
            return;
        }

        //创建内容容器
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(transform);

        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentParent = contentRect;

        //设置锚点到左侧顶部（垂直排列）
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(0f, 1f);
        contentRect.pivot = new Vector2(0f, 1f);
        contentRect.anchoredPosition = new Vector2(10, -10);
        contentRect.sizeDelta = new Vector2(200, 2000);

        //添加垂直布局组件
        VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        layout.childAlignment = TextAnchor.UpperLeft;
    }

    //初始化UI
    private void InitializeUI()
    {
        // 如果 contentParent 为 null，先尝试查找已存在的 Content 对象
        if (contentParent == null)
        {
            Transform existingContent = transform.Find("Content");
            if (existingContent != null)
            {
                contentParent = existingContent;
                Debug.Log("ActionOrderUI: 找到已存在的 Content 对象，使用它作为 contentParent");
            }
            else
            {
                Debug.LogWarning("ActionOrderUI: contentParent 未设置，正在自动创建...");
                CreateContentParent();
            }
        }

        if (gm == null)
        {
            Debug.LogError("ActionOrderUI: GameManager is null!");
            return;
        }

        //清空现有槽位
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        //为每个单位创建槽位
        if (gm.allUnits != null)
        {
            Debug.Log($"ActionOrderUI: 开始初始化，单位数量: {gm.allUnits.Count}");
            int createdCount = 0;
            foreach (Unit unit in gm.allUnits)
            {
                if (unit != null && unit.health > 0)
                {
                    CreateUnitSlot(unit);
                    createdCount++;
                }
            }
            Debug.Log($"ActionOrderUI: 创建了 {createdCount} 个槽位");
        }
        else
        {
            Debug.LogWarning("ActionOrderUI: gm.allUnits is null!");
        }
    }

    //创建单位槽位
    private void CreateUnitSlot(Unit unit)
    {
        GameObject slotObj;

        //如果有预制体，使用预制体；否则自动创建
        if (unitSlotPrefab != null)
        {
            slotObj = Instantiate(unitSlotPrefab, contentParent);
        }
        else
        {
            slotObj = CreateSlotPrefab();
        }

        ActionOrderSlot slot = slotObj.GetComponent<ActionOrderSlot>();
        if (slot == null)
        {
            slot = slotObj.AddComponent<ActionOrderSlot>();
        }
        slot.Initialize(unit);
        slots.Add(slot);
    }

    //自动创建单位槽位预制体
    private GameObject CreateSlotPrefab()
    {
        //创建主面板
        GameObject slotObj = new GameObject("UnitSlot");
        slotObj.transform.SetParent(contentParent);

        RectTransform slotRect = slotObj.AddComponent<RectTransform>();
        slotRect.sizeDelta = new Vector2(150, 80);

        Image slotBg = slotObj.AddComponent<Image>();
        slotBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); //半透明黑色背景

        //创建单位名称文本
        GameObject nameObj = new GameObject("UnitNameText");
        nameObj.transform.SetParent(slotObj.transform);
        RectTransform nameRect = nameObj.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.5f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = new Vector2(5, 0);
        nameRect.offsetMax = new Vector2(-5, -5);
        Text nameText = nameObj.AddComponent<Text>();
        //尝试使用Unity默认字体
        if (nameText.font == null)
        {
            //如果没有默认字体，尝试加载LegacyRuntime字体
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont != null)
            {
                nameText.font = defaultFont;
            }
        }
        nameText.text = "Unit"; //设置默认文本
        nameText.fontSize = 16;
        nameText.alignment = TextAnchor.MiddleCenter;
        nameText.color = Color.white;
        nameText.supportRichText = false;
        nameText.raycastTarget = false; //不需要射线检测

        //创建行动条背景
        GameObject barBgObj = new GameObject("ActionBarBg");
        barBgObj.transform.SetParent(slotObj.transform);
        RectTransform barBgRect = barBgObj.AddComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0, 0);
        barBgRect.anchorMax = new Vector2(1, 0.4f);
        barBgRect.offsetMin = new Vector2(5, 5);
        barBgRect.offsetMax = new Vector2(-5, -5);
        Image barBg = barBgObj.AddComponent<Image>();
        barBg.color = new Color(0.3f, 0.3f, 0.3f, 1f); //深灰色背景

        //创建行动条填充
        GameObject barFillObj = new GameObject("ActionBarFill");
        barFillObj.transform.SetParent(barBgObj.transform);
        RectTransform barFillRect = barFillObj.AddComponent<RectTransform>();
        barFillRect.anchorMin = Vector2.zero;
        barFillRect.anchorMax = Vector2.one;
        barFillRect.offsetMin = Vector2.zero;
        barFillRect.offsetMax = Vector2.zero;
        Image barFill = barFillObj.AddComponent<Image>();
        barFill.type = Image.Type.Filled;
        barFill.fillMethod = Image.FillMethod.Horizontal;
        barFill.color = Color.blue;

        //创建行动值文本
        GameObject valueObj = new GameObject("ActionValueText");
        valueObj.transform.SetParent(slotObj.transform);
        RectTransform valueRect = valueObj.AddComponent<RectTransform>();
        valueRect.anchorMin = new Vector2(0, 0);
        valueRect.anchorMax = new Vector2(1, 0.4f);
        valueRect.offsetMin = new Vector2(5, 5);
        valueRect.offsetMax = new Vector2(-5, -5);
        Text valueText = valueObj.AddComponent<Text>();
        //尝试使用Unity默认字体
        if (valueText.font == null)
        {
            //如果没有默认字体，尝试加载LegacyRuntime字体
            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont != null)
            {
                valueText.font = defaultFont;
            }
        }
        valueText.text = "0/100"; //设置默认文本
        valueText.fontSize = 12;
        valueText.alignment = TextAnchor.MiddleCenter;
        valueText.color = Color.white;
        valueText.supportRichText = false;
        valueText.raycastTarget = false; //不需要射线检测

        //创建高亮边框
        GameObject highlightObj = new GameObject("HighlightFrame");
        highlightObj.transform.SetParent(slotObj.transform);
        RectTransform highlightRect = highlightObj.AddComponent<RectTransform>();
        highlightRect.anchorMin = Vector2.zero;
        highlightRect.anchorMax = Vector2.one;
        highlightRect.offsetMin = Vector2.zero;
        highlightRect.offsetMax = Vector2.zero;
        Image highlight = highlightObj.AddComponent<Image>();
        highlight.color = new Color(1f, 1f, 0f, 0.5f); //半透明黄色
        highlightObj.SetActive(false); //默认不显示

        //设置ActionOrderSlot组件的引用
        ActionOrderSlot slot = slotObj.AddComponent<ActionOrderSlot>();
        slot.unitNameText = nameText;
        slot.actionBarFill = barFill;
        slot.actionValueText = valueText;
        slot.highlightFrame = highlightObj;

        //确保所有子对象都激活
        nameObj.SetActive(true);
        valueObj.SetActive(true);
        barBgObj.SetActive(true);
        barFillObj.SetActive(true);

        Debug.Log($"ActionOrderUI: 创建了单位槽位，名称文本: {nameText != null}, 行动值文本: {valueText != null}");

        return slotObj;
    }

    //更新UI显示
    private void UpdateUI()
    {
        if (gm == null || contentParent == null)
        {
            return;
        }

        //获取按行动值排序的单位列表
        List<Unit> sortedUnits = gm.GetAllUnitsSortedByActionValue();

        if (sortedUnits == null || sortedUnits.Count == 0)
        {
            //如果没有单位，隐藏所有槽位
            foreach (var slot in slots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    slot.gameObject.SetActive(false);
                }
            }
            return;
        }

        //使用字典来跟踪每个单位对应的槽位
        Dictionary<Unit, ActionOrderSlot> unitToSlotMap = new Dictionary<Unit, ActionOrderSlot>();
        List<ActionOrderSlot> slotsToRemove = new List<ActionOrderSlot>();

        foreach (var slot in slots)
        {
            if (slot != null && slot.currentUnit != null)
            {
                //检查单位是否死亡
                if (slot.currentUnit.health <= 0)
                {
                    //单位已死亡，标记为需要移除
                    slotsToRemove.Add(slot);
                }
                else
                {
                    unitToSlotMap[slot.currentUnit] = slot;
                }
            }
            else if (slot != null)
            {
                //槽位没有关联单位，也需要移除
                slotsToRemove.Add(slot);
            }
        }

        //销毁死亡单位的UI槽位
        foreach (var slotToRemove in slotsToRemove)
        {
            slots.Remove(slotToRemove);
            if (slotToRemove != null && slotToRemove.gameObject != null)
            {
                Destroy(slotToRemove.gameObject);
            }
        }

        //确保槽位数量足够
        while (slots.Count < sortedUnits.Count)
        {
            Unit tempUnit = sortedUnits[slots.Count];
            CreateUnitSlot(tempUnit);
            if (slots.Count > 0 && slots[slots.Count - 1] != null)
            {
                unitToSlotMap[tempUnit] = slots[slots.Count - 1];
            }
        }

        //移除多余的槽位
        while (slots.Count > sortedUnits.Count)
        {
            ActionOrderSlot lastSlot = slots[slots.Count - 1];
            if (lastSlot != null && lastSlot.currentUnit != null)
            {
                unitToSlotMap.Remove(lastSlot.currentUnit);
            }
            slots.RemoveAt(slots.Count - 1);
            if (lastSlot != null && lastSlot.gameObject != null)
            {
                Destroy(lastSlot.gameObject);
            }
        }

        //按照排序后的单位顺序，重新组织槽位
        List<ActionOrderSlot> newSlots = new List<ActionOrderSlot>();

        for (int i = 0; i < sortedUnits.Count; i++)
        {
            Unit targetUnit = sortedUnits[i];
            if (targetUnit == null) continue;

            //查找或创建对应的槽位
            ActionOrderSlot targetSlot = null;
            if (unitToSlotMap.ContainsKey(targetUnit))
            {
                targetSlot = unitToSlotMap[targetUnit];
            }
            else
            {
                CreateUnitSlot(targetUnit);
                targetSlot = slots[slots.Count - 1];
                unitToSlotMap[targetUnit] = targetSlot;
            }

            //更新槽位显示
            targetSlot.UpdateSlot(targetUnit, i, gm);
            newSlots.Add(targetSlot);
        }

        //更新slots列表
        slots = newSlots;

        //关键：按照新顺序重新排列所有Transform
        //必须从后往前设置，避免索引冲突
        for (int i = newSlots.Count - 1; i >= 0; i--)
        {
            if (newSlots[i] != null && newSlots[i].transform != null)
            {
                newSlots[i].transform.SetAsLastSibling();
            }
        }

        //然后按照正确顺序设置
        for (int i = 0; i < newSlots.Count; i++)
        {
            if (newSlots[i] != null && newSlots[i].transform != null)
            {
                newSlots[i].transform.SetSiblingIndex(i);
            }
        }
    }
}


