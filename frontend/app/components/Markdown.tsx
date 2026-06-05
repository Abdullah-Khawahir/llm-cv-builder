import { memo } from "react";
import ReactMarkdown from "react-markdown";
import remarkBreaks from "remark-breaks";
import remarkGfm from "remark-gfm";

export type Props = React.HTMLAttributes<HTMLDivElement> & {
  content: string;
};

const remarkPlugins = [remarkGfm , remarkBreaks];

function _Markdown({ content, }: { content: string }) {
  return (
    <div className="overflow-x-auto w-full scrollbar-thin">
      <ReactMarkdown remarkPlugins={remarkPlugins}>
        {content}
      </ReactMarkdown>
    </div>
  );
}

export const Markdown = memo(_Markdown);
