import { memo } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";

export type Props = React.HTMLAttributes<HTMLDivElement> & {
  content: string;
};

const remarkPlugins = [remarkGfm];

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
