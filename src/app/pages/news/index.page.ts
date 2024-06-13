import { Component, OnInit } from '@angular/core';
import { injectContentFiles } from '@analogjs/content';
import PostAttributes from '../../post-attributes';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-news',
  standalone: true,
  imports: [RouterLink, CommonModule],
  template: `
  <div class="mx-auto px-7 rounded-lg shadow-lg select-none container">
    <h1 class="font-['Conthrax'] text-4xl mb-1 text-center">News</h1>
    <div class="container mx-auto flex-wrap">
      @for (post of posts;track post.attributes.slug) {
        <a [routerLink]="['/news/', post.attributes.slug]">
          <div class="container bg-[rgba(0,0,0,0.7)] mb-3 hover:bg-[#191919] rounded-br-lg rounded-tl-xl  max-w-4xl mx-auto">
            <div class="container text-gray-300 bg-[#030303] rounded-tl-xl pl-2 pr-2 pt-1 pb-1 border-l-2 border-[red]">
              <h2 class="font-['Conthrax'] text-xl md:text-3xl pl-1 text-white">{{ post.attributes.title }} </h2>
              <p class="text-xs ml-1 mt-1 text-[rgba(255,70,0,0.8)] mx-auto">{{post.attributes.date | date:'longDate'}}</p>
            </div>
            <div class="container ml-3 pb-1">
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
export default class NewsIndexComponent implements OnInit {

  readonly posts = injectContentFiles<PostAttributes>((contentFile) => {
    return contentFile.attributes.type === 'news';
  });

  ngOnInit(): void {
    this.posts.sort((a, b) => {
      if (a.attributes.date === undefined || b.attributes.date === undefined) {
        if (a.attributes.date !== undefined) {
          return -1;
        }

        return b.attributes.title < a.attributes.title ? 1 : b.attributes.title > a.attributes.title ? -1 : 0;
      }

      return a.attributes.date < b.attributes.date ? 1 : -1
    });
  }
}
