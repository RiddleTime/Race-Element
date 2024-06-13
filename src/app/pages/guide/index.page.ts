import { Component, OnInit } from '@angular/core';
import { injectContentFiles } from '@analogjs/content';
import PostAttributes from '../../post-attributes';
import { Route, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouteMeta } from '@analogjs/router';

export const routeMeta: RouteMeta = {
  meta: [
    {
      property: 'og:title',
      content: 'Race Element - Guides',
    },
  ],
};

@Component({
  selector: 'app-guides',
  standalone: true,
  imports: [RouterLink, CommonModule],
  template: `
  <div class="mx-auto rounded-lg shadow-lg select-none container max-w-4xl px-3">
    <h1 class="font-['Conthrax'] text-4xl mb-2 md:mb-1 text-center">Guides</h1>
    <div class="container mx-auto flex-wrap">
      @for (post of posts;track post.attributes.slug) {
        <a [routerLink]="['/guide/', post.attributes.slug]">
          <div class="container bg-[rgba(0,0,0,0.7)] mb-3 hover:bg-[#191919] hover:border-[transparent] hover:border-l-2 rounded-br-lg rounded-tl-xl  max-w-4xl mx-auto text-pretty">
            <div class="container text-gray-300 bg-[#030303] rounded-tl-xl pl-2 pr-2 pt-1 pb-1 border-l-2 border-[red]">
              <h2 class="font-['Conthrax'] text-xl md:text-3xl pl-1 text-white">{{ post.attributes.title }} </h2>
            </div>
            <div class="container ml-3 pr-[1em] pb-1 text-pretty">
              <p class="text-sm md:text-lg ml-1 mr-1 text-[rgba(255,255,255,0.78)]">{{ post.attributes.description }}</p>
            </div>
          </div>
        </a>
      }
    </div>
  </div>
  `,
  styles: [
    `

    `,
  ],
})
export default class GuidesIndexComponent implements OnInit {

  readonly posts = injectContentFiles<PostAttributes>((contentFile) => {
    return contentFile.attributes.type === 'guide';
  });

  ngOnInit(): void {
    this.posts.sort((a, b) => {
      return b.attributes.title < a.attributes.title ? 1 : b.attributes.title > a.attributes.title ? -1 : 0;
    });
  }
}
