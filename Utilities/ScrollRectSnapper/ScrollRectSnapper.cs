using System;
using System.Collections;
using System.Collections.Generic;
using OneTon.Animation;
using OneTon.Logging;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace OneTon.Utilities
{

    public class ScrollRectSnapper : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        /// <summary>
        /// The direction the content RectTransform has moved in since our last snap
        /// </summary>
        private enum ScrollDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        private static LogService logger = LogService.Get<ScrollRectSnapper>(LogLevel.Debug);
        private ScrollRect scrollRect;
        public ScrollRect ScrollRect
        {
            get
            {
                return scrollRect;
            }
        }

        [SerializeField] private float dragThreshold = 1f; // minimum drag distance required to initiate a scroll
        [SerializeField] private float velocityThreshold = 10f; // minimum velocity allowed before snapping to closest child
        [SerializeField] private float snapThreshold = 10f; // distance threshold to end snap tween
        [SerializeField] private int minChildChange = 0; // minimum num of child indexes we shift our focus when user initiates a scroll
        [SerializeField] private int maxChildChange = 0; // maximum num of child indexes we shift our focus when user initiates a scroll
        [SerializeField] private float maxDuration = 1f;
        [SerializeField] private bool stopExact = false;
        [SerializeField][ReadOnly] private List<RectTransform> activeChildren;
        [SerializeField][ReadOnly] RectTransform currentSnappedChild;
        Vector3 startingPosition;
        bool snapOnScrollValueChanged = false;
        bool isDragging = false;
        bool isAnimating = false;
        Coroutine currentTween;

        void Awake()
        {
            activeChildren = new();
            scrollRect = GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

        void Update()
        {
            RectTransform child;
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                child = scrollRect.content.GetChild(i) as RectTransform;
                if (child.gameObject.activeInHierarchy != activeChildren.Contains(child))
                {
                    GetActiveChildren();
                    if (currentSnappedChild == null)
                    {
                        SnapToClosestChild();
                    }
                    break;
                }
            }
        }

        private void GetActiveChildren()
        {
            logger.Trace();
            activeChildren = new();
            RectTransform child;
            for (int i = 0; i < scrollRect.content.childCount; i++)
            {
                child = scrollRect.content.GetChild(i) as RectTransform;
                if (child.gameObject.activeInHierarchy)
                {
                    activeChildren.Add(child);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            snapOnScrollValueChanged = true;
            startingPosition = scrollRect.content.localPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            logger.Trace();

            isDragging = false;

            float distance = Vector3.Magnitude(scrollRect.content.localPosition - startingPosition);
            float duration = GetDuration(distance);

            if (distance < dragThreshold)
            {
                SnapToChild(currentSnappedChild, duration);
            }
        }

        void OnScrollValueChanged(Vector2 value)
        {

            if (!snapOnScrollValueChanged || isDragging || isAnimating) return;

            logger.Debug($"OnScrollValueChanged: {value}");
            logger.Debug($"scrollRect.velocity.magnitude: {scrollRect.velocity.magnitude}");

            if (scrollRect.velocity.magnitude < velocityThreshold)
            {
                SnapToClosestChild();
                return;
            }

            if (stopExact)
            {
                int lowIndex = Math.Max(GetChildIndexAboveSnapPoint(), 0);
                int highIndex = Math.Min(GetChildIndexBelowSnapPoint(), activeChildren.Count - 1);
                int currentSnapIndex = activeChildren.IndexOf(currentSnappedChild);
                ScrollDirection scrollDirection = GetScrollDirection();
                int minSnappableIndex = GetMinSnappableIndex(scrollDirection, currentSnapIndex);
                int maxSnappableIndex = GetMaxSnappableIndex(scrollDirection, currentSnapIndex);

                if (scrollDirection == ScrollDirection.Up && highIndex >= maxSnappableIndex)
                {
                    SnapToChild(activeChildren[maxSnappableIndex], GetDuration(Math.Abs(activeChildren[maxSnappableIndex].localPosition.y + scrollRect.content.localPosition.y)));
                }
                else if (scrollDirection == ScrollDirection.Down && lowIndex <= minSnappableIndex)
                {
                    SnapToChild(activeChildren[minSnappableIndex], GetDuration(Math.Abs(activeChildren[minSnappableIndex].localPosition.y + scrollRect.content.localPosition.y)));
                }
            }
        }

        private void SnapToClosestChild()
        {
            int lowIndex = Math.Max(GetChildIndexAboveSnapPoint(), 0);
            int highIndex = Math.Min(GetChildIndexBelowSnapPoint(), activeChildren.Count - 1);
            logger.Debug($"lowIndex: {lowIndex}");
            logger.Debug($"highIndex: {highIndex}");

            if (currentSnappedChild != null)
            {
                int currentSnapIndex = activeChildren.IndexOf(currentSnappedChild);

                ScrollDirection scrollDirection = GetScrollDirection();
                logger.Debug($"scrollDirection: {scrollDirection}");

                int minSnappableIndex = GetMinSnappableIndex(scrollDirection, currentSnapIndex);
                logger.Debug($"minSnappableIndex: {minSnappableIndex}");

                int maxSnappableIndex = GetMaxSnappableIndex(scrollDirection, currentSnapIndex);
                logger.Debug($"maxSnappableIndex: {maxSnappableIndex}");

                if (scrollDirection == ScrollDirection.Up)
                {
                    lowIndex = Math.Max(lowIndex, minSnappableIndex);
                    highIndex = Math.Max(lowIndex, Math.Min(highIndex, maxSnappableIndex));
                }
                else if (scrollDirection == ScrollDirection.Down)
                {
                    highIndex = Math.Min(highIndex, maxSnappableIndex);
                    lowIndex = Math.Min(highIndex, Math.Max(lowIndex, minSnappableIndex));
                }


                logger.Debug($"lowIndex: {lowIndex}");
                logger.Debug($"highIndex: {highIndex}");
            }

            float lowIndexDistance = Math.Abs(activeChildren[lowIndex].localPosition.y + scrollRect.content.localPosition.y);
            float highIndexDistance = Math.Abs(activeChildren[highIndex].localPosition.y + scrollRect.content.localPosition.y);

            logger.Debug($"lowIndexDistance: {lowIndexDistance}");
            logger.Debug($"highIndexDistance: {highIndexDistance}");
            if (lowIndexDistance < highIndexDistance)
            {
                SnapToChild(activeChildren[lowIndex], GetDuration(lowIndexDistance));
            }
            else
            {
                SnapToChild(activeChildren[highIndex], GetDuration(highIndexDistance));
            }
        }

        private ScrollDirection GetScrollDirection()
        {

            ScrollDirection scrollDirection;
            if (scrollRect.content.localPosition.y >= -currentSnappedChild.localPosition.y)
            {
                scrollDirection = ScrollDirection.Up;
            }
            else
            {
                scrollDirection = ScrollDirection.Down;
            }

            logger.Debug($"scrollDirection: {scrollDirection}");

            return scrollDirection;
        }

        private int GetChildIndexBelowSnapPoint()
        {
            int i = 0;
            for (; i < activeChildren.Count; i++)
            {
                if (-activeChildren[i].localPosition.y >= scrollRect.content.transform.localPosition.y)
                {
                    break;
                }
            }
            return i;
        }

        private int GetChildIndexAboveSnapPoint()
        {
            int i = activeChildren.Count - 1;
            for (; i >= 0; i--)
            {
                if (-activeChildren[i].localPosition.y <= scrollRect.content.transform.localPosition.y)
                {
                    break;
                }
            }
            return i;
        }

        private int GetMinSnappableIndex(ScrollDirection scrollDirection, int currentSnapIndex)
        {
            if (scrollDirection == ScrollDirection.Up)
            {
                return Math.Min(activeChildren.Count - 1, currentSnapIndex + minChildChange);
            }
            else if (scrollDirection == ScrollDirection.Down)
            {
                return maxChildChange > 0 ? Math.Max(0, currentSnapIndex - maxChildChange) : 0;
            }
            else
            {
                throw new Exception($"GetMinSnappableIndex() for ScrollDirection {scrollDirection} not implemented!");
            }
        }

        private int GetMaxSnappableIndex(ScrollDirection scrollDirection, int currentSnapIndex)
        {
            if (scrollDirection == ScrollDirection.Up)
            {
                return maxChildChange > 0 ? Math.Min(activeChildren.Count - 1, currentSnapIndex + maxChildChange) : activeChildren.Count - 1;
            }
            else if (scrollDirection == ScrollDirection.Down)
            {
                return Math.Max(0, currentSnapIndex - minChildChange);
            }
            else
            {
                throw new Exception($"GetMaxSnappableIndex() for ScrollDirection {scrollDirection} not implemented!");
            }
        }

        private float GetDuration(float distance)
        {
            return EasingFunction.EaseOutCirc(0, maxDuration, distance / scrollRect.content.rect.height);
        }

        public void SnapToChild(RectTransform target, float duration)
        {
            logger.Debug($"SnapToChild({target.name}, {duration})");

            snapOnScrollValueChanged = false;
            isAnimating = true;
            scrollRect.StopMovement();

            Vector2 childLocalPosition = target.localPosition;

            Vector2 targetPosition = new Vector2(-childLocalPosition.x, -childLocalPosition.y);

            logger.Debug($"targetPosition: {targetPosition}");

            Vector2 clampedTargetPosition = new Vector2(
                Mathf.Clamp(targetPosition.x, scrollRect.content.rect.min.x, scrollRect.content.rect.max.x),
                Mathf.Clamp(targetPosition.y, scrollRect.content.rect.max.y, -scrollRect.content.rect.min.y)
            );

            logger.Debug($"clampedTargetPosition: {clampedTargetPosition}");

            if (currentTween != null)
            {
                StopCoroutine(currentTween);
            }

            currentTween = StartCoroutine(Routine());

            IEnumerator Routine()
            {
                float t = 0;
                while (t < duration)
                {
                    if (Vector3.Distance(scrollRect.content.localPosition, clampedTargetPosition) < snapThreshold)
                    {
                        break;
                    }

                    t += Time.deltaTime;
                    scrollRect.content.localPosition = Vector3.Lerp(scrollRect.content.localPosition, clampedTargetPosition, t);

                    yield return new WaitForEndOfFrame();
                }

                scrollRect.velocity = Vector2.zero;
                scrollRect.content.localPosition = clampedTargetPosition;
                currentSnappedChild = target;
                isAnimating = false;
            }
        }
    }
}