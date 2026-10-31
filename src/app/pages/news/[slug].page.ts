import { Component, inject, OnInit, ViewEncapsulation } from '@angular/core';
import { injectContent, MarkdownComponent } from '@analogjs/content';
import { AsyncPipe, DatePipe } from '@angular/common';
import PostAttributes from '../../post-attributes';
import { Meta } from '@angular/platform-browser';

@Component({
    selector: 'app-news-post',
    imports: [AsyncPipe, MarkdownComponent, DatePipe],
    template: `
    @if (post | async; as post) {
    <article class="rounded-lg container mx-auto max-w-4xl px-3">
      <h1 class="text-xl md:text-3xl font-['Conthrax'] select-none dark:text-gray-300 dark:bg-black rounded-tl-xl border-l-2 pl-2 pr-2 pt-1 pb-1 border-red-800">
        <a href="/news">News</a> > {{post.attributes.title}}
      </h1>
      <div class="container dark:bg-[#050505] pl-3 pr-[1em] pt-2 rounded-br-xl">
        <p class="select-none text-sm mb-3">{{post.attributes.date !== undefined? (post.attributes.date | date:'longDate') : ''}}</p>
        <analog-markdown [content]="post.content" />
      </div>
    </article>
    }
  `,
    styles: [
        `
    `,
    ],
    encapsulation: ViewEncapsulation.None
})
export default class NewsSlugComponent implements OnInit {
  private readonly meta = inject(Meta);
  readonly post = injectContent<PostAttributes>({
    param: 'slug',
    subdirectory: 'news',
  });

  ngOnInit(): void {
    this.post.forEach(x => {
      this.meta.updateTag({ name: `og:url`, content: `https://race.elementfuture.com/news/${x.attributes.slug}` });
      this.meta.updateTag({ name: `og:title`, content: `Race Element - News | ${x.attributes.title}` });
      this.meta.updateTag({ name: `og:description`, content: `${x.attributes.description}` });
      this.meta.updateTag({ name: `twitter:title`, content: `Race Element - News | ${x.attributes.title}` });
    });
  }
}
