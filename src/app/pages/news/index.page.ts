import { Component } from '@angular/core';
import { DatePipe } from '@angular/common';
import { injectContentFiles } from '@analogjs/content';
import PostAttributes from '../../post-attributes';
import { RouterLink } from '@angular/router';
import { RouteMeta } from '@analogjs/router';

export const routeMeta: RouteMeta = {
  meta: [
    {
      property: 'og:title',
      content: 'Race Element - News',
    },
    {
      property: 'twitter:title',
      content: 'Race Element - News',
    },
  ],
};

@Component({
    selector: 'app-news',
    imports: [RouterLink, DatePipe],
    template: `
  <div class="mx-auto rounded-lg shadow-lg select-none container max-w-4xl px-3">
    <div class="container mx-auto flex-wrap">
      @for (post of posts;track post.attributes.slug) {
        <a [routerLink]="['/news/', post.attributes.slug]"
           class="block border-b border-white/10 border-l-[3px] border-l-transparent hover:border-l-[orangered] hover:bg-white/5 py-3 px-3 transition-colors">
          <p class="text-xs text-[orangered]">{{ post.attributes.date | date:'longDate' }}</p>
          <h2 class="font-['Conthrax'] text-lg md:text-xl text-white mt-0.5">{{ post.attributes.title }}</h2>
          <p class="text-sm text-white/70 mt-1">{{ post.attributes.description }}</p>
        </a>
      }
    </div>
  </div>
  `,
    styles: [
        `

    `,
    ]
})
export default class NewsIndexComponent {
  readonly posts = injectContentFiles<PostAttributes>((contentFile) => {
    return contentFile.attributes.type === 'news';
  }).sort((a, b) => {
    if (a.attributes.date === undefined || b.attributes.date === undefined) {
      if (a.attributes.date !== undefined) {
        return -1;
      }

      return b.attributes.title < a.attributes.title ? 1 : b.attributes.title > a.attributes.title ? -1 : 0;
    }

    return a.attributes.date < b.attributes.date ? 1 : -1;
  });
}
