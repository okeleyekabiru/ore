import { useState } from 'react';
import { 
  Sparkles, 
  Send,
  Download,
  Copy,
  RefreshCw,
  Save,
  Settings,
  Image,
  Video,
  FileText,
  Wand2
} from 'lucide-react';

const contentTypes = [
  { id: 'post', label: 'Social Media Post', icon: FileText, description: 'Generate engaging social media content' },
  { id: 'image', label: 'Image Caption', icon: Image, description: 'Create captions for your images' },
  { id: 'video', label: 'Video Script', icon: Video, description: 'Write compelling video scripts' },
  { id: 'blog', label: 'Blog Article', icon: FileText, description: 'Generate full blog articles' },
];

const toneOptions = ['Professional', 'Casual', 'Friendly', 'Energetic', 'Formal', 'Playful'];
const platformOptions = ['Instagram', 'Facebook', 'Twitter', 'LinkedIn', 'TikTok', 'YouTube'];

export const ContentGeneratorPage = () => {
  const [selectedType, setSelectedType] = useState('post');
  const [prompt, setPrompt] = useState('');
  const [tone, setTone] = useState('Professional');
  const [platform, setPlatform] = useState('Instagram');
  const [generatedContent, setGeneratedContent] = useState('');
  const [isGenerating, setIsGenerating] = useState(false);
  const [showAdvanced, setShowAdvanced] = useState(false);

  const handleGenerate = async () => {
    setIsGenerating(true);
    
    // Simulate AI content generation
    setTimeout(() => {
      const mockContent = {
        post: `🌟 Exciting news! We're thrilled to announce our latest innovation that's about to revolutionize your daily routine. 

✨ Key highlights:
• Cutting-edge technology
• User-friendly design  
• Sustainable approach
• Premium quality

Join thousands of satisfied customers who have already transformed their experience. What are you waiting for?

#Innovation #Technology #Lifestyle #Premium #Quality`,
        
        image: `Captured in this moment: pure inspiration and endless possibilities. ✨

Sometimes the most beautiful things come from the simplest experiences. This image tells a story of [describe the scene/emotion], reminding us that every day holds something special.

What story does this image tell you? Share your thoughts below! 👇

#Photography #Inspiration #Moments #Beauty #Life`,
        
        video: `[HOOK - First 3 seconds]
"Wait until you see what happens next..."

[INTRODUCTION - 0:03-0:15]  
Hey everyone! Today I'm going to show you something that will completely change how you think about [topic].

[MAIN CONTENT - 0:15-0:45]
Here's the step-by-step process:
1. First, we start with...
2. Then, we add...
3. Finally, the magic happens when...

[CALL TO ACTION - 0:45-0:60]
If you found this helpful, smash that like button and follow for more amazing content like this!

What would you like to see next? Drop your suggestions in the comments!`,
        
        blog: `# The Future is Here: Revolutionary Changes in [Your Industry]

## Introduction
In today's rapidly evolving landscape, staying ahead of the curve isn't just an advantage—it's essential. As we navigate through unprecedented changes, one thing remains clear: innovation is the key to success.

## The Current Landscape
The market has seen significant shifts in recent years, with consumers demanding more personalized, efficient, and sustainable solutions. This has led to a complete transformation in how businesses operate and engage with their audiences.

## Key Trends to Watch
1. **Technology Integration**: Seamless integration of AI and automation
2. **Sustainability Focus**: Eco-conscious decision making
3. **Personalization**: Tailored experiences for every user
4. **Community Building**: Creating meaningful connections

## What This Means for You
These changes represent both challenges and opportunities. By embracing innovation and staying adaptable, you can position yourself at the forefront of this transformation.

## Conclusion
The future belongs to those who are willing to evolve. Are you ready to take the next step?`
      };

      setGeneratedContent(mockContent[selectedType as keyof typeof mockContent] || mockContent.post);
      setIsGenerating(false);
    }, 2000);
  };

  const copyToClipboard = () => {
    navigator.clipboard.writeText(generatedContent);
    // Show toast notification
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="p-6">
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            AI Content Generator
          </h1>
          <p className="text-gray-600 dark:text-gray-400 mt-2">
            Create engaging content powered by artificial intelligence
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
          {/* Input Panel */}
          <div className="space-y-6">
            {/* Content Type Selection */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Content Type
              </h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {contentTypes.map((type) => (
                  <button
                    key={type.id}
                    onClick={() => setSelectedType(type.id)}
                    className={`p-4 rounded-lg border-2 text-left transition-all ${
                      selectedType === type.id
                        ? 'border-primary-500 bg-primary-50 dark:bg-primary-900/20'
                        : 'border-gray-200 dark:border-gray-700 hover:border-gray-300 dark:hover:border-gray-600'
                    }`}
                  >
                    <div className="flex items-center space-x-3">
                      <type.icon className={`h-6 w-6 ${
                        selectedType === type.id 
                          ? 'text-primary-600 dark:text-primary-400' 
                          : 'text-gray-400'
                      }`} />
                      <div>
                        <h4 className="font-medium text-gray-900 dark:text-white">
                          {type.label}
                        </h4>
                        <p className="text-sm text-gray-500 dark:text-gray-400">
                          {type.description}
                        </p>
                      </div>
                    </div>
                  </button>
                ))}
              </div>
            </div>

            {/* Prompt Input */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Content Brief
              </h3>
              <textarea
                value={prompt}
                onChange={(e) => setPrompt(e.target.value)}
                placeholder="Describe what you want to create. Be specific about your topic, audience, and goals..."
                rows={6}
                className="w-full p-3 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 focus:border-transparent dark:bg-gray-700 dark:text-white resize-none"
              />
            </div>

            {/* Settings */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                  Settings
                </h3>
                <button
                  onClick={() => setShowAdvanced(!showAdvanced)}
                  className="text-sm text-primary-600 hover:text-primary-700 dark:text-primary-400"
                >
                  <Settings className="h-4 w-4 inline mr-1" />
                  Advanced
                </button>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Tone
                  </label>
                  <select
                    value={tone}
                    onChange={(e) => setTone(e.target.value)}
                    className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white"
                  >
                    {toneOptions.map((option) => (
                      <option key={option} value={option}>{option}</option>
                    ))}
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Platform
                  </label>
                  <select
                    value={platform}
                    onChange={(e) => setPlatform(e.target.value)}
                    className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white"
                  >
                    {platformOptions.map((option) => (
                      <option key={option} value={option}>{option}</option>
                    ))}
                  </select>
                </div>

                {showAdvanced && (
                  <div className="pt-4 border-t border-gray-200 dark:border-gray-700">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          Length
                        </label>
                        <select className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white">
                          <option>Short</option>
                          <option>Medium</option>
                          <option>Long</option>
                        </select>
                      </div>
                      <div>
                        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                          Creativity
                        </label>
                        <select className="w-full p-2 border border-gray-300 dark:border-gray-600 rounded-lg focus:ring-2 focus:ring-primary-500 dark:bg-gray-700 dark:text-white">
                          <option>Conservative</option>
                          <option>Balanced</option>
                          <option>Creative</option>
                        </select>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Generate Button */}
            <button
              onClick={handleGenerate}
              disabled={!prompt.trim() || isGenerating}
              className="w-full flex items-center justify-center space-x-2 bg-primary-600 hover:bg-primary-700 disabled:bg-gray-400 text-white px-6 py-3 rounded-lg transition-colors font-medium"
            >
              {isGenerating ? (
                <>
                  <RefreshCw className="h-5 w-5 animate-spin" />
                  <span>Generating...</span>
                </>
              ) : (
                <>
                  <Wand2 className="h-5 w-5" />
                  <span>Generate Content</span>
                </>
              )}
            </button>
          </div>

          {/* Output Panel */}
          <div className="space-y-6">
            {/* Generated Content */}
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900 dark:text-white">
                  Generated Content
                </h3>
                {generatedContent && (
                  <div className="flex items-center space-x-2">
                    <button
                      onClick={copyToClipboard}
                      className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                      title="Copy to clipboard"
                    >
                      <Copy className="h-4 w-4" />
                    </button>
                    <button
                      className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                      title="Download as text file"
                    >
                      <Download className="h-4 w-4" />
                    </button>
                    <button
                      className="p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300"
                      title="Save to library"
                    >
                      <Save className="h-4 w-4" />
                    </button>
                  </div>
                )}
              </div>

              {generatedContent ? (
                <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                  <pre className="whitespace-pre-wrap text-gray-900 dark:text-white font-sans text-sm leading-relaxed">
                    {generatedContent}
                  </pre>
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center py-12 text-center">
                  <Sparkles className="h-12 w-12 text-gray-400 mb-4" />
                  <h4 className="text-lg font-medium text-gray-900 dark:text-white mb-2">
                    Ready to create amazing content?
                  </h4>
                  <p className="text-gray-500 dark:text-gray-400">
                    Enter your content brief and click generate to see the magic happen
                  </p>
                </div>
              )}
            </div>

            {/* Content Actions */}
            {generatedContent && (
              <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
                <h4 className="text-md font-semibold text-gray-900 dark:text-white mb-4">
                  What's next?
                </h4>
                <div className="space-y-3">
                  <button className="w-full flex items-center space-x-3 p-3 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                    <Send className="h-5 w-5 text-primary-600 dark:text-primary-400" />
                    <div className="text-left">
                      <p className="font-medium text-gray-900 dark:text-white">
                        Send for Approval
                      </p>
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        Submit to your approval workflow
                      </p>
                    </div>
                  </button>
                  
                  <button className="w-full flex items-center space-x-3 p-3 border border-gray-200 dark:border-gray-700 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
                    <Sparkles className="h-5 w-5 text-purple-600 dark:text-purple-400" />
                    <div className="text-left">
                      <p className="font-medium text-gray-900 dark:text-white">
                        Regenerate Variation
                      </p>
                      <p className="text-sm text-gray-500 dark:text-gray-400">
                        Create a different version
                      </p>
                    </div>
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};