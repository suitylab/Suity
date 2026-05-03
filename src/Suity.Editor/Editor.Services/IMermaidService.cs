using Suity.Drawing;
using Suity.Editor.Documents;
using System.Drawing;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Specifies the types of diagrams supported by Mermaid syntax.
/// </summary>
/// <remarks>Use this enumeration to indicate the desired diagram type when generating or rendering Mermaid
/// diagrams. The available types correspond to the primary diagram formats supported by Mermaid.</remarks>
public enum MermaidGraphType
{
    /// <summary>
    /// Represents a flowchart consisting of nodes and connections that define a process or workflow.
    /// </summary>
    /// <remarks>Use this class to model, manipulate, or analyze the structure of a flowchart in applications
    /// such as diagram editors, workflow engines, or process visualization tools. The flowchart typically contains
    /// nodes representing steps or decisions and edges representing transitions between them.</remarks>
    Flowchart,

    /// <summary>
    /// Represents a class diagram that illustrates the structure of a system by showing its classes,
    /// attributes, methods, and relationships.
    /// </summary>
    /// <remarks>Use this class to model, manipulate, or analyze class diagrams in applications such as UML
    /// design tools, software architecture visualizers, or code generation systems. The class diagram typically
    /// contains classes with their properties and methods, along with associations, inheritances, and dependencies
    /// between them.</remarks>
    Class,

    /// <summary>
    /// Represents a sequence diagram that depicts the interactions between objects or components over time.
    /// </summary>
    /// <remarks>Use this class to model, manipulate, or analyze sequence diagrams in applications such as
    /// software design tools, communication protocol analyzers, or system interaction visualizers.
    /// The sequence diagram typically contains lifelines representing objects and messages exchanged
    /// between them in a time-ordered sequence.</remarks>
    Sequence,

    EntityRelationship,

    State,

    /// <summary>
    /// Represents a hierarchical structure of interconnected ideas or concepts, typically used for brainstorming,
    /// organizing information, or visualizing relationships.
    /// </summary>
    /// <remarks>A mind map consists of nodes that represent individual ideas, with connections indicating
    /// relationships or associations between them. This class can be used to model, manipulate, or traverse such
    /// structures in applications that require visual or logical organization of information.</remarks>
    Mindmap,

    /// <summary>
    /// Represents an architecture diagram that illustrates the high-level structure and components of a system.
    /// </summary>
    Architecture,

    /// <summary>
    /// Represents a block diagram that visualizes the components of a system and their interconnections.
    /// </summary>
    Block,

    /// <summary>
    /// Represents a C4 model diagram that visualizes the architecture of software systems at different levels
    /// </summary>
    C4,

    /// <summary>
    /// Represents a Gantt chart that illustrates a project schedule, showing tasks, durations, and dependencies.
    /// </summary>
    Gantt,

    /// <summary>
    /// Represents a Git graph that visualizes the commit history and branching structure of a Git repository.
    /// </summary>
    Git,

    /// <summary>
    /// Represents a Kanban board that visualizes work items and their status in a workflow.
    /// </summary>
    Kanban,

    /// <summary>
    /// Represents a network packet diagram that visualizes the bytes and structure of network packets.
    /// </summary>
    Packet,

    /// <summary>
    /// Represents a pie chart that visualizes data as proportional segments of a circle.
    /// </summary>
    Pie,

    /// <summary>
    /// Represents a quadrant chart that visualizes data points in a two-dimensional space divided into four quadrants.
    /// </summary>
    Quadrant,

    /// <summary>
    /// Represents a radar chart that visualizes multivariate data in a circular format with axes radiating from the center.
    /// </summary>
    Radar,

    /// <summary>
    /// Represents a requirement diagram that visualizes requirements and their relationships in a system.
    /// </summary>
    Requirement,

    /// <summary>
    /// Represents a Sankey diagram that visualizes flows and their quantities between different entities.
    /// </summary>
    Sankey,

    /// <summary>
    /// Represents a timeline diagram that visualizes events or activities along a chronological axis.
    /// </summary>
    Timeline,

    /// <summary>
    /// Represents a treemap diagram that visualizes hierarchical data as nested rectangles.
    /// </summary>
    Treemap,

    /// <summary>
    /// Represents a user journey diagram that visualizes the steps and interactions of a user with a system or service.
    /// </summary>
    UserJourney,

    /// <summary>
    /// Represents an XY chart that visualizes data points in a two-dimensional Cartesian coordinate system.
    /// </summary>
    XY,

    /// <summary>
    /// Represents a ZenUML diagram that visualizes user stories and interactions in a simplified UML format.
    /// </summary>
    ZenUML,

}

/// <summary>
/// Defines methods for generating and handling Mermaid diagrams from articles or text prompts.
/// </summary>
/// <remarks>Implementations of this interface provide functionality to generate Mermaid diagram code, retrieve
/// diagram images or live URLs, and identify Mermaid code blocks within text. This service is intended to support
/// applications that need to create, display, or process Mermaid diagrams dynamically.</remarks>
public interface IMermaidService
{
    /// <summary>
    /// Generates a Mermaid diagram for the specified article using the given graph type.
    /// </summary>
    /// <param name="article">The article for which to generate the Mermaid diagram. Cannot be null.</param>
    /// <param name="graphType">The type of Mermaid graph to generate for the article.</param>
    /// <param name="prompt">An optional prompt to guide the diagram generation process. If provided, this prompt
    /// will influence the content and structure of the generated diagram.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the article with the generated
    /// Mermaid diagram included.</returns>
    Task<IArticle> GenerateMermaid(IArticle article, MermaidGraphType graphType, string prompt = null);

    /// <summary>
    /// Generates a Mermaid diagram definition based on the specified prompt and graph type.
    /// </summary>
    /// <param name="input">A textual description of the diagram to generate. The prompt should clearly describe the entities,
    /// relationships, or structure to be represented in the Mermaid diagram.</param>
    /// <param name="graphType">The type of Mermaid graph to generate, such as flowchart, sequence diagram, or class diagram. Determines the
    /// style and syntax of the resulting diagram.</param>
    /// <param name="prompt">An optional additional prompt to further refine or guide the diagram generation process. This can include
    /// specific instructions or constraints to influence the output.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a string with the Mermaid diagram
    /// definition corresponding to the provided prompt and graph type.</returns>
    Task<string> GenerateMermaid(string input, MermaidGraphType graphType, string prompt = null);

    /// <summary>
    /// Determines whether the specified input string represents a Mermaid code block.
    /// </summary>
    /// <param name="input">The input string to evaluate. Cannot be null.</param>
    /// <returns>true if the input string is recognized as a Mermaid code block; otherwise, false.</returns>
    bool IsMermaidCodeBlock(string input);

    /// <summary>
    /// Returns the live streaming URL corresponding to the specified input identifier.
    /// </summary>
    /// <param name="input">The identifier or source string used to determine the live stream URL. Cannot be null or empty.</param>
    /// <returns>A string containing the live streaming URL associated with the specified input. Returns null if no matching URL
    /// is found.</returns>
    string GetLiveUrl(string input);

    /// <summary>
    /// Returns the URL of the image associated with the specified input.
    /// </summary>
    /// <param name="input">The identifier or name used to look up the corresponding image. Cannot be null or empty.</param>
    /// <returns>A string containing the URL of the image associated with the input. Returns null if no image is found for the
    /// specified input.</returns>
    string GetImageUrl(string input);

    /// <summary>
    /// Generates a bitmap image representing the specified Mermaid diagram definition.
    /// </summary>
    /// <param name="input">The Mermaid diagram definition as a string. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a bitmap image of the rendered
    /// Mermaid diagram.</returns>
    Task<BitmapDef> GenerateMermaidBitmap(string input);

    /// <summary>
    /// Retrieves a cached bitmap image for the specified Mermaid diagram definition if available.
    /// </summary>
    /// <param name="input">The Mermaid diagram definition as a string to look up in the cache.</param>
    /// <returns>A bitmap image of the Mermaid diagram if found in cache; otherwise, implementation-specific behavior.</returns>
    BitmapDef GetCachedMermaidBitmap(string input);

    bool IsImageGenerating();
}
