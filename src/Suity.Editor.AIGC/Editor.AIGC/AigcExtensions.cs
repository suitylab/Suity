using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MarkedNet;
using Suity.Editor.Documents;
using Suity.Editor.Transferring;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Json;
using Suity.UndoRedos;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Suity.Editor.AIGC;

public static class AigcExtensions
{
    /// <summary>
    /// Adds a user message to the conversation with optional attachments and export type instructions.
    /// </summary>
    /// <param name="handler">The conversation handler.</param>
    /// <param name="msg">The message content.</param>
    /// <param name="attachments">Optional attachment sets to include as JSON code blocks.</param>
    /// <param name="exportType">The export content type for appending specific instructions.</param>
    /// <param name="display">Whether to display the message in the UI.</param>
    /// <returns>The full message string including attachments and instructions.</returns>
    public static string AddUserMessage(this IConversationHandler handler, string msg, AttachmentSet[] attachments, AigcExportContentTypes exportType = AigcExportContentTypes.None, bool display = true)
    {
        var builder = new StringBuilder(msg);

        if (attachments?.Any() == true)
        {
            foreach (var attachment in attachments)
            {
                string code = attachment.GetJson();
                if (string.IsNullOrWhiteSpace(code))
                {
                    continue;
                }

                builder.AppendLine();
                builder.AppendLine("```json");
                builder.Append(code);
                builder.AppendLine();
                builder.AppendLine("```");
            }
        }

        switch (exportType)
        {
            case AigcExportContentTypes.TypeEdit:
                builder.AppendLine();
                builder.Append("Please output a brief summary and a complete and rigorous TypeEdit document in SuityJson.");
                break;

            case AigcExportContentTypes.DataEdit:
                builder.AppendLine();
                builder.Append("Please output a brief summary and a complete and rigorous DataEdit document in SuityJson.");
                break;
        }

        string fullMsg = builder.ToString();

        if (display)
        {
            handler.AddMyMessage(msg, config =>
            {
                config.AddButtons
                (
                    string.Empty,
                    new ConversationButton { Text = "Copy", CallBack = CreateTextCopyButton(fullMsg ?? msg) },
                    new ConversationButton
                    {
                        Text = "Re-edit",
                        CallBack = () =>
                        {
                            AttachmentSet[] attClone = null;

                            if (attachments != null)
                            {
                                attClone = new AttachmentSet[attachments.Length];

                                for (int i = 0; i < attachments.Length; i++)
                                {
                                    attClone[i] = attachments[i].Clone();
                                }
                            }

                            LLmService.Instance.SetInput(msg, attClone);
                        }
                    }
                );
            });
        }

        return fullMsg;
    }

    /// <summary>
    /// Renders a Markdown-formatted message and adds it to the dialog using the MarkedNet lexer.
    /// </summary>
    /// <param name="config">The dialog message configuration.</param>
    /// <param name="msg">The Markdown-formatted message content.</param>
    public static void AddMarkdownMessage(this IDialogMessage config, string msg)
    {
        if (string.IsNullOrWhiteSpace(msg))
        {
            return;
        }

        var markdown = "# Heading Content";

        // Manually call Lexer to get Token list
        var tokens = MarkedNet.Lexer.Lex(markdown, new Options());


        var inList = new Stack<int>();
        bool inListItem = false;
        bool inFirstItem = false;

        foreach (var token in tokens.Tokens)
        {
            switch (token.Type)
            {
                case "paragraph":
                    config.AddText(token.Text);
                    break;

                case "code":
                    config.ConfigCodeBlock(token.Text);
                    break;

                case "list_start":
                    if (token.Ordered)
                    {
                        inList.Push(1);
                    }
                    else
                    {
                        inList.Push(0);
                    }

                    inListItem = false;
                    break;

                case "list_end":
                    if (inList.Count > 0)
                    {
                        inList.Pop();
                    }
                    inListItem = false;
                    break;

                case "list_item_start":
                    if (inList.Count > 0)
                    {
                        inListItem = true;
                        inFirstItem = true;
                    }
                    break;

                case "loose_item_start":
                    if (inList.Count > 0)
                    {
                        inListItem = true;
                        inFirstItem = true;
                    }
                    break;

                case "list_item_end":
                    inListItem = false;
                    inFirstItem = false;
                    if (inList.Count > 0 && inList.Peek() > 0)
                    {
                        int num = inList.Pop();
                        inList.Push(num + 1);
                    }
                    break;

                case "text":
                    if (inList.Count > 0 && inListItem && inFirstItem)
                    {
                        int num = inList.Peek();
                        if (num > 0)
                        {
                            //config.AddText($"{new string(' ', inList.Count * 2)}{num}. {token.Text}");
                            config.AddText($"{new string(' ', inList.Count * 2)}- {token.Text}");
                        }
                        else
                        {
                            config.AddText($"{new string(' ', inList.Count * 2)}- {token.Text}");
                        }

                        inFirstItem = false;
                    }
                    else
                    {
                        config.AddText(token.Text);
                    }
                    break;

                case "space":
                    config.AddLine();
                    break;

                case "heading":
                    config.AddText(token.Text);
                    break;

                default:
                    config.AddCode($"({token.Type}) {token.Text}");
                    break;
            }
        }

        config.AddLine();
        config.AddButton("Copy Original", () =>
        {
            EditorUtility.SetSystemClipboardText(msg).ContinueWith(t => 
            {
                if (t.Result)
                {
                    Logs.LogInfo("Original text copied to clipboard.");
                }
            });
        });
    }

    /// <summary>
    /// Renders a Markdown-formatted message and adds it to the dialog using the Markdig parser.
    /// </summary>
    /// <param name="config">The dialog message configuration.</param>
    /// <param name="msg">The Markdown-formatted message content.</param>
    public static void AddMarkdigMessage(this IDialogMessage config, string msg)
    {
        if (string.IsNullOrWhiteSpace(msg))
        {
            return;
        }

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var result = Markdown.Parse(msg, pipeline);

        config.ProcessMarkdigBlocks(result);

        config.AddLine();
        config.AddButton("Copy Original", () =>
        {
            EditorUtility.SetSystemClipboardText(msg).ContinueWith(t => 
            {
                if (t.Result)
                {
                    Logs.LogInfo("Original text copied to clipboard.");
                }
            });
        });
    }

    private static void ProcessMarkdigBlocks(this IDialogMessage config, IEnumerable<Block> blocks, int depth = 0, bool ordered = false, int? orderN = null)
    {
        foreach (var block in blocks)
        {
            switch (block)
            {
                case ParagraphBlock paragraph:
                    if (paragraph.Inline is ContainerInline inLines)
                    {
                        foreach (Inline inline in inLines)
                        {
                            if (inline is LineBreakInline)
                            {
                                continue;
                            }

                            if (orderN is int order)
                            {
                                if (order > 0)
                                {
                                    config.AddText($"{new string(' ', depth * 2)}{order}. {inline}");
                                }
                                else
                                {
                                    config.AddText($"{new string(' ', depth * 2)}- {inline}");
                                }

                                orderN = null;
                            }
                            else
                            {
                                config.AddText($"{new string(' ', depth * 2)}{inline}");
                            }
                        }
                    }
                    break;

                case ListBlock list:
                    if (list.Count > 0)
                    {
                        config.ProcessMarkdigBlocks(list, depth, list.IsOrdered);
                    }
                    break;

                case ListItemBlock listItem:
                    if (listItem.Count > 0)
                    {
                        config.ProcessMarkdigBlocks(listItem, depth + 1, ordered, listItem.Order);
                    }
                    break;

                case FencedCodeBlock fencedCodeBlock:
                    config.ConfigCodeBlock(string.Join("\r\n", fencedCodeBlock.Lines.Lines));
                    break;

                case ThematicBreakBlock line:
                    config.AddLine();
                    break;

                default:
                    config.AddText($"[{block.GetType().Name}]");
                    break;
            }
        }
    }

    private static void ConfigCodeBlock(this IDialogMessage config, string code)
    {
        JsonDataReader reader = null;
        string docType = null;

        try
        {
            do
            {
                reader = new JsonDataReader(code);

                string format = reader.Node("@format").ReadString();
                if (format != "SuityJson")
                {
                    reader = null;
                    break;
                }

                docType = reader.Node("@doc").ReadString();
                if (string.IsNullOrWhiteSpace(docType))
                {
                    reader = null;
                    break;
                }
            } while (false);
        }
        catch (Exception)
        {
        }

        if (reader != null)
        {
            config.AddButtons
            (
                $"{docType} Code",
                new ConversationButton { Text = "Apply", CallBack = () => HandleApplyCode(reader) },
                new ConversationButton { Text = "View", CallBack = CreateCodeViewButton(code) },
                new ConversationButton { Text = "Copy", CallBack = CreateTextCopyButton(code) }
            );
        }
        else
        {
            config.AddButtons
            (
                "Code",
                new ConversationButton { Text = "View", CallBack = CreateCodeViewButton(code) },
                new ConversationButton { Text = "Copy", CallBack = CreateTextCopyButton(code) }
            );

            return;
        }
    }

    private static Action CreateTextCopyButton(string text)
    {
        return () =>
        {
            EditorUtility.SetSystemClipboardText(text ?? string.Empty).ContinueWith(t => 
            {
                if (t.Result)
                {
                    Logs.LogInfo("Copied to clipboard.");
                }
            });
        };
    }

    private static Action CreateCodeViewButton(string code)
    {
        return () =>
        {
            DialogUtility.ShowTextBlockDialogAsync("View Code", code, string.Empty);
        };
    }

    private static Action CreateCodeActionButton(string code, Action<string> codeBtnCallBack = null)
    {
        return () =>
        {
            codeBtnCallBack?.Invoke(code);
        };
    }

    /// <summary>
    /// Parses a JSON code string and applies the code to the active document.
    /// </summary>
    /// <param name="code">The JSON code string to parse and apply.</param>
    public static void HandleApplyCode(string code)
    {
        JsonDataReader reader;
        try
        {
            reader = new JsonDataReader(code);
        }
        catch (Exception err)
        {
            err.LogError("Json parse failed");
            return;
        }

        if (reader is null)
        {
            return;
        }

        HandleApplyCode(reader);
    }

    /// <summary>
    /// Applies data from a data reader to the active document, supporting undo/redo operations.
    /// </summary>
    /// <param name="reader">The data reader containing the data to apply.</param>
    public static void HandleApplyCode(IDataReader reader)
    {
        if (reader is null)
        {
            throw new ArgumentNullException(nameof(reader));
        }

        var doc = DocumentViewManager.Current.ActiveDocument?.Content;
        if (doc is null)
        {
            Logs.LogError("Please open a document first.");
            return;
        }

        if (ContentTransfer<DataRW>.GetTransfer(doc.GetType()) is null)
        {
            Logs.LogError("Current document does not support data entry.");

            return;
        }

        try
        {
            var action = new DocumentJsonSetterAction(doc, doc, reader);

            var undoRedo = doc.View?.GetService<UndoRedoManager>();
            if (undoRedo != null)
            {
                undoRedo.Do(action);
            }
            else
            {
                action.Do();
                doc.View?.RefreshView();
            }

            Logs.LogInfo($"Data applied to {doc}.");
        }
        catch (Exception err)
        {
            err.LogError("Data entry failed.");
        }
    }

    //public static ILLmChat CreateChatConversation(this OpenAIModels model, FunctionContext context = null)
    //{
    //    //switch (model)
    //    //{
    //    //    case OpenAIModels.ChatGPTTurbo:
    //    //    case OpenAIModels.ChatGPTTurbo_16k:
    //    //    case OpenAIModels.ChatGPTTurbo_Preview:
    //    //    case OpenAIModels.GPT4:
    //    //    case OpenAIModels.GPT4_32k_Context:
    //    //    case OpenAIModels.GPT4_Turbo:
    //    //        return OkGoDoItGPTChat.CreateChat(model, context);
    //    //}

    //    return null;
    //}

    //public static ILLmCall CreateChatCall(this OpenAIModels model, FunctionContext context = null)
    //{
    //    //switch (model)
    //    //{
    //    //    case OpenAIModels.ChatGPTTurbo:
    //    //    case OpenAIModels.ChatGPTTurbo_16k:
    //    //    case OpenAIModels.ChatGPTTurbo_Preview:
    //    //    case OpenAIModels.GPT4:
    //    //    case OpenAIModels.GPT4_32k_Context:
    //    //    case OpenAIModels.GPT4_Turbo:
    //    //        return OkGoDoItGPTCall.CreateCall(model, context);
    //    //}

    //    return null;
    //}


    /// <summary>
    /// Writes content to a file within the workspace, resolving the full path relative to the workspace master directory.
    /// </summary>
    /// <param name="workSpace">The workspace context.</param>
    /// <param name="filePath">The relative file path within the workspace.</param>
    /// <param name="content">The content to write to the file.</param>
    public static void WriteWorkSpaceFile(this WorkSpace workSpace, string filePath, string content)
    {
        filePath ??= string.Empty;
        filePath = filePath.Trim().Replace('\\', '/').TrimStart('.', '/');
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new NullReferenceException($"File path is empty.");
        }

        string fileFullPath = workSpace.MakeMasterFullPath(filePath);

        FileUtils.Write(fileFullPath, content);
    }

    /// <summary>
    /// Retrieves all files recursively from a specified base path within the workspace.
    /// </summary>
    /// <param name="workSpace">The workspace context.</param>
    /// <param name="basePath">The relative base directory path within the workspace.</param>
    /// <returns>An array of FileInfo objects representing all files found, or an empty array if the directory does not exist.</returns>
    public static FileInfo[] GetFiles(this WorkSpace workSpace, string basePath)
    {
        basePath ??= string.Empty;
        basePath = basePath.Trim().Replace('\\', '/').TrimStart('.', '/');

        string dirFullPath = workSpace.MakeMasterFullPath(basePath);

        var dirInfo = new DirectoryInfo(dirFullPath);
        if (dirInfo.Exists)
        {
            return dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
        }
        else
        {
            return [];
        }
    }

    /// <summary>
    /// Retrieves all file paths recursively from a specified base path within the workspace, returned as relative paths.
    /// </summary>
    /// <param name="workSpace">The workspace context.</param>
    /// <param name="basePath">The relative base directory path within the workspace.</param>
    /// <returns>An array of relative file paths, or an empty array if the directory does not exist.</returns>
    public static string[] GetRelativeFilePaths(this WorkSpace workSpace, string basePath)
    {
        basePath ??= string.Empty;
        basePath = basePath.Trim().Replace('\\', '/').TrimStart('.', '/');

        string dirFullPath = workSpace.MakeMasterFullPath(basePath);

        var dirInfo = new DirectoryInfo(dirFullPath);
        if (dirInfo.Exists)
        {
            return dirInfo.GetFiles("*.*", SearchOption.AllDirectories).Select(o => 
            {
                string rPath = workSpace.MakeMasterRelativePath(o.FullName);

                return rPath.Replace('\\', '/');
            }).ToArray();
        }
        else
        {
            return [];
        }
    }
}