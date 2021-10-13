using AutoMapper.Internal;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace osu_replay_renderer_netcore
{
    static class DrawablesUtils
    {
        public static void RemoveRecursive(this Container<Drawable> container, Predicate<Drawable> predicate)
        {
            container.RemoveAll(predicate);
            container.ForEach(drawable =>
            {
                if (drawable is Container<Drawable> container2) RemoveRecursive(container2, predicate);
                else if (drawable is FillFlowContainer fillFlow) RemoveRecursive(fillFlow, predicate);
            });
        }

        public static Drawable GetInternalChild(CompositeDrawable drawable)
        {
            MethodInfo internalChildMethod = typeof(CompositeDrawable).GetDeclaredMethod("get_InternalChild");
            return internalChildMethod.Invoke(drawable, null) as Drawable;
        }

        public static IReadOnlyList<Drawable> GetInternalChildren(CompositeDrawable drawable)
        {
            MethodInfo internalChildMethod = typeof(CompositeDrawable).GetDeclaredMethod("get_InternalChildren");
            return internalChildMethod.Invoke(drawable, null) as IReadOnlyList<Drawable>;
        }
    }
}
