using System.Windows;
using System.Windows.Automation;

namespace ATEDNIULI_NET8.Services
{
    public class UIAutomationService
    {
        public List<Point> GetClickableItems()
        {
            var clickableItems = new List<Point>();

            // Get the AutomationElement for the currently focused element.
            AutomationElement focusedElement = AutomationElement.FocusedElement;

            if (focusedElement == null)
            {
                return clickableItems;
            }

            // Find the top-level window that contains the focused element.
            AutomationElement targetWindow = FindParentWindow(focusedElement);

            if (targetWindow == null)
            {
                return clickableItems;
            }

            // Now, perform the search only within that specific window.
            System.Windows.Automation.Condition condition = new AndCondition(
                new PropertyCondition(AutomationElement.IsControlElementProperty, true),
                new PropertyCondition(AutomationElement.IsEnabledProperty, true),
                new PropertyCondition(AutomationElement.IsInvokePatternAvailableProperty, true)
            );

            // Find all matching elements within the target window.
            AutomationElementCollection elements = targetWindow.FindAll(TreeScope.Descendants, condition);

            foreach (AutomationElement element in elements)
            {
                Rect boundingRect = element.Current.BoundingRectangle;
                Point centerPoint = new Point(boundingRect.Left + boundingRect.Width / 2,
                                              boundingRect.Top + boundingRect.Height / 2);
                clickableItems.Add(centerPoint);
            }

            return clickableItems;
        }

        // helper method to find the current focused/active window
        private AutomationElement FindParentWindow(AutomationElement element)
        {
            TreeWalker walker = TreeWalker.ControlViewWalker;

            AutomationElement parent = walker.GetParent(element);

            while (parent != null && !parent.Current.ControlType.Equals(ControlType.Window))
            {
                parent = walker.GetParent(parent);
            }

            return parent;
        }
    }
}
