namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/Grid Layout Advanced", 156)]
    [SelectionBase]
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class GridLayoutAdvanced : LayoutGroup, ILayoutSelfController
    {
        [SerializeField] int CellsPerLine = 3;
        [SerializeField] int Spacing = 0; 
        [SerializeField] float CellAspectRatio = 1;
        
        [System.Serializable]
        enum BasicDirection
        {
            Vertical,
            Horizontal,
        }
        int fixedCellsPerLine, fixedSpacing;
        float fixedCellAspectRatio;
        [SerializeField] BasicDirection direction;
        BasicDirection old_direction;
        Vector2 cellSize;
        int linesCount;
        float referenceRectSize, requestedRectSize;
        
        protected override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();
        }
        
        public override void CalculateLayoutInputVertical()
        {
            return;
        }

        public override void SetLayoutHorizontal()
        {
            ReplaceCells();
        }

        public override void SetLayoutVertical()
        {
            ReplaceCells();
        }
        
        void ReplaceCells()
        {
            FixUserInput();
            TryDetectDirectionChange();
            if (direction == BasicDirection.Vertical)
            {
                ReplaceCellsVertical();
            }
            else 
            {
                ReplaceCellsHorizontal();
            }
        }
        
        void FixUserInput()
        {
            fixedCellsPerLine = Mathf.Clamp(CellsPerLine, 1, int.MaxValue);
            fixedSpacing = Mathf.Clamp(Spacing, 0, int.MaxValue);
            fixedCellAspectRatio = Mathf.Clamp(CellAspectRatio, 0.1f, float.MaxValue);
        }
        
        void TryDetectDirectionChange()
        {
            if (old_direction == direction) return;
            old_direction = direction;
            if (direction == BasicDirection.Vertical)
            {
                rectTransform.anchorMin = Vector2.up;
                rectTransform.anchorMax = Vector2.one;
            }
            else 
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.up;
            }
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
        }
        
        void ReplaceCellsHorizontal()
        {
            CalculateBasics();
            ChangeAndRegisterMySize();
            PlaceChilds();
            
            void CalculateBasics()
            {
                linesCount = Mathf.CeilToInt(rectChildren.Count/ (float) fixedCellsPerLine);
                referenceRectSize = rectTransform.rect.height - padding.vertical - fixedSpacing * (fixedCellsPerLine - 1);
                float CellHeight = referenceRectSize / (float) fixedCellsPerLine;
                cellSize = new Vector2(CellHeight  * fixedCellAspectRatio, CellHeight);
            }
            
            void ChangeAndRegisterMySize()
            {
                requestedRectSize = linesCount * cellSize.x + fixedSpacing * (linesCount - 1) + padding.horizontal;
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
                rectTransform.sizeDelta = new Vector2(requestedRectSize, rectTransform.sizeDelta.y);
            }
            
            void PlaceChilds()
            {
                float HeightShift = - padding.top - cellSize.y;
                float WidthShift = padding.left;
                //pivot compensation
                HeightShift += rectTransform.rect.height * (1 - rectTransform.pivot.y);
                WidthShift -= requestedRectSize * rectTransform.pivot.x;
                //
                bool LastLine = false;
                int ChildIncrement = 0;
                for(int Line = 0; Line < linesCount; Line ++)
                {
                    LastLine = Line == linesCount-1;
                    if (LastLine)
                    {
                        HeightShift -= GetHeightShiftAtLastLine();
                    }
                    for (int Cell = 0; Cell < fixedCellsPerLine; Cell ++)
                    {
                        if (ChildIncrement >= rectChildren.Count) continue;
                        Vector2 CellBasePos = new Vector2(WidthShift, HeightShift - Cell * (cellSize.y + fixedSpacing));
                        Vector2 PivotRelativePos = CellBasePos + cellSize * rectChildren[ChildIncrement].pivot;
                        rectChildren[ChildIncrement].localPosition = PivotRelativePos;
                        rectChildren[ChildIncrement].sizeDelta = cellSize;
                        ChildIncrement++;
                    }
                    WidthShift += cellSize.x;
                    WidthShift += fixedSpacing;
                }
            }
            
            float GetHeightShiftAtLastLine()
            {
                if (childAlignment == TextAnchor.UpperLeft   ||
                    childAlignment == TextAnchor.UpperCenter ||
                    childAlignment == TextAnchor.UpperRight)
                {
                    return 0;
                }
                float Multiplier = 1; //if bottom align
                if (childAlignment == TextAnchor.MiddleLeft   ||
                    childAlignment == TextAnchor.MiddleCenter ||
                    childAlignment == TextAnchor.MiddleRight)
                {
                    Multiplier = 0.5f;
                }
                int EmptyCells = fixedCellsPerLine - (rectChildren.Count % fixedCellsPerLine);
                if (EmptyCells == fixedCellsPerLine) return 0;
                float EmptySpace = referenceRectSize * (EmptyCells / (float) fixedCellsPerLine) + fixedSpacing * EmptyCells;
                return EmptySpace * Multiplier;
            }
        }
        
        void ReplaceCellsVertical()
        {
            CalculateBasics();
            ChangeAndRegisterMySize();
            PlaceChilds();
            
            void CalculateBasics()
            {
                linesCount = Mathf.CeilToInt(rectChildren.Count/ (float) fixedCellsPerLine);
                referenceRectSize = rectTransform.rect.width - padding.horizontal - fixedSpacing * (fixedCellsPerLine - 1);
                float CellWidth = referenceRectSize / (float) fixedCellsPerLine;
                cellSize = new Vector2(CellWidth, CellWidth / fixedCellAspectRatio);
            }
            
            void ChangeAndRegisterMySize()
            {
                requestedRectSize = linesCount * cellSize.y + fixedSpacing * (linesCount - 1) + padding.top + padding.bottom;
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, requestedRectSize);
            }
            
            void PlaceChilds()
            {
                float HeightShift = - padding.top;
                float WidthShift = padding.right;
                //pivot compensation
                HeightShift += requestedRectSize * (1 - rectTransform.pivot.y);
                WidthShift -= rectTransform.rect.width * rectTransform.pivot.x;
                //
                bool LastLine = false;
                int ChildIncrement = 0;
                for(int Line = 0; Line < linesCount; Line ++)
                {
                    LastLine = Line == linesCount-1;
                    HeightShift -= cellSize.y;
                    if (LastLine)
                    {
                        WidthShift += GetWidthShiftAtLastLine();
                    }
                    for (int Cell = 0; Cell < fixedCellsPerLine; Cell ++)
                    {
                        if (ChildIncrement >= rectChildren.Count) continue;
                        Vector2 CellBasePos = new Vector2(WidthShift + Cell * (cellSize.x + fixedSpacing), HeightShift);
                        Vector2 PivotRelativePos = CellBasePos + cellSize * rectChildren[ChildIncrement].pivot;
                        rectChildren[ChildIncrement].localPosition = PivotRelativePos;
                        rectChildren[ChildIncrement].sizeDelta = cellSize;
                        ChildIncrement++;
                    }
                    HeightShift -= fixedSpacing;
                }
            }
            
            float GetWidthShiftAtLastLine()
            {
                if (childAlignment ==  TextAnchor.UpperLeft ||
                    childAlignment == TextAnchor.MiddleLeft ||
                    childAlignment ==  TextAnchor.LowerLeft)
                {
                    return 0;
                }
                float Multiplier = 1; //if right align
                if (childAlignment ==  TextAnchor.UpperCenter ||
                    childAlignment == TextAnchor.MiddleCenter ||
                    childAlignment ==  TextAnchor.LowerCenter)
                {
                    Multiplier = 0.5f;
                }
                int EmptyCells = fixedCellsPerLine - (rectChildren.Count % fixedCellsPerLine);
                if (EmptyCells == fixedCellsPerLine) return 0;
                float EmptySpace = referenceRectSize * (EmptyCells / (float) fixedCellsPerLine) + fixedSpacing * EmptyCells;
                return EmptySpace * Multiplier;
            }
        }
    }
}