using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Milutools.Logger;
using Milutools.Milutools.General;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Milutools.SceneRouter
{
    public class SceneRouter : MonoBehaviour
    {
        private const char PathSeparator = '/';
        
        internal static bool Enabled = false;
        
        internal readonly static Dictionary<EnumIdentifier, SceneRouterNode> Nodes = new();
        internal static SceneRouterNode RootNode, CurrentNode;

        internal static GameObject LoadingAnimatorPrefab;
        internal static object Parameters;

        /// <summary>
        /// When calling the Back() method at the root node, should the game exit directly?
        /// </summary>
        public static bool QuitOnRootNode { get; set; } = true;
        
        public static void Setup(IEnumerable<SceneRouterNode> nodes)
        {
            if (Enabled)
            {
                DebugLog.LogError("Duplicated setup is not allowed.");
                return;
            }
            
            LoadingAnimatorPrefab = Resources.Load<GameObject>("BlackFade");

            foreach (var node in nodes)
            {
                if (node.IsRoot)
                {
                    if (RootNode != null)
                    {
                        DebugLog.LogError("There should be only an root node.");
                        return;
                    }

                    RootNode = node;
                }
                Nodes.Add(node.Identifier, node);
            }

            CurrentNode = nodes.FirstOrDefault(x => x.Scene == SceneManager.GetActiveScene().name);
            if (CurrentNode == null)
            {
                DebugLog.LogWarning($"Current scene '{SceneManager.GetActiveScene().name}' is not included in this scene notes, " +
                                    $"SceneRouter.Back() won't work properly in this scene.");
            }

#if UNITY_EDITOR
            SceneManager.activeSceneChanged += (_, scene) =>
            {
                if (CurrentNode != null && CurrentNode.Scene == scene.name)
                {
                    return;
                }
                DebugLog.LogError("You seem to be trying to manage the scenes on your own, " +
                                  "but this might hinder the normal functioning of the scene router, " +
                                  "and it doesn't align with the design principles.");
            };
#endif
            
            Enabled = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ValidateLoadingPrefab(GameObject prefab)
        {
            var result = prefab.TryGetComponent<LoadingAnimator>(out _);
            if (!result)
            {
                DebugLog.LogError("The loading prefab must has a LoadingAnimator component on its root object.");
            }
            return result;
        }

        public static T FetchParameters<T>()
            => (T)Parameters;
        
        public static void SetLoadingAnimator(GameObject prefab)
        {
            if (!ValidateLoadingPrefab(prefab))
            {
                return;
            }

            LoadingAnimatorPrefab = prefab;
        }
        
        private static SceneRouterNode Node<T>(T identifier, string path, string scene, bool isRoot) where T : Enum
        {
            return new SceneRouterNode()
            {
                Identifier = EnumIdentifier.Wrap(identifier),
                Path = path.Split(PathSeparator),
                FullPath = path,
                Scene = scene,
                IsRoot = isRoot
            };
        }

        public static SceneRouterNode Root<T>(T identifier, string scene) where T : Enum
            => Node(identifier, "", scene, true);
        
        public static SceneRouterNode Node<T>(T identifier, string path, string scene) where T : Enum
            => Node(identifier, path, scene, false);

        private static SceneRouterContext GoTo(SceneRouterNode node, GameObject loadingPrefab = null)
        {
            if (!Enabled)
            {
                DebugLog.LogError("Scene router is not enabled, please configure the scene nodes first.");
                return null;
            }
            var prefab = loadingPrefab ?? LoadingAnimatorPrefab;
            var go = Instantiate(prefab);
            var animator = go.GetComponent<LoadingAnimator>();
            animator.TargetScene = node.Scene;
            go.SetActive(true);

            Parameters = null;
            CurrentNode = node;

            return new SceneRouterContext();
        }
        
        public static SceneRouterContext GoTo<T>(T scene, GameObject loadingPrefab = null) where T : Enum
        {
            var key = EnumIdentifier.Wrap(scene);
            if (!Nodes.ContainsKey(key))
            {
                DebugLog.LogError($"The specific scene node '{key}' is not found.");
            }
            
            return GoTo(Nodes[key], loadingPrefab);
        }

        public static SceneRouterContext Back(GameObject loadingPrefab = null)
        {
            if (CurrentNode.Path.Length < 2)
            {
                if (QuitOnRootNode && CurrentNode == RootNode)
                {
                    DebugLog.LogWarning("At this point, the compiled game will terminate the process.");
                    Application.Quit();
                    return null;
                }
                return GoTo(RootNode, loadingPrefab);
            }
            var path = string.Join(PathSeparator, CurrentNode.Path[..^1]);
            var node = Nodes.Values.FirstOrDefault(x => x.FullPath == path);
            if (node == null)
            {
                DebugLog.LogWarning($"The parent node of scene node '{CurrentNode.Identifier}' is not configured, the router will navigate to the root node.");
                node = RootNode;
            }
            return GoTo(node, loadingPrefab);
        }
    }
}
