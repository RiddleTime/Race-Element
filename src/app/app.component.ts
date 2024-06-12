import { map } from 'rxjs';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `
    <div class="container mx-auto flex flex-col self-center">
      <a href="/" class="self-center text-center font-['Conthrax'] text-5xl md:text-7xl text-[orangered] select-none hover:text-[orangered] drop-shadow-[0_0px_1.5px_rgba(0.01,0.01,0.01,0.95)]">
        Race Element
      </a>
      <nav class="text-lg font-['Conthrax'] max-w-md md:max-w-3xl mx-auto md:text-2xl flex flex-wrap flex-col md:flex-row">
        <a href="/news" class="mx-auto mb-2 md:mb-0 text-[white] hover:text-[red] hover:bg-[rgba(0,0,0,0.8)] ml-1 mr-1 pl-2 pr-2 bg-[rgba(0.2,0.2,0.2)] rounded-tl-lg rounded-br-lg">News</a>
        <a href="/guide" class="mx-auto mb-2 md:mb-0 text-[white] hover:text-[red] hover:bg-[rgba(0,0,0,0.8)] ml-1 mr-1 pl-2 pr-2 bg-[rgba(0.2,0.2,0.2)] rounded-tl-lg rounded-br-lg">Guides</a>
        <a href="/guide/features" class="mx-auto mb-2 md:mb-0 text-[white] hover:text-[red] hover:bg-[rgba(0,0,0,0.8)] ml-1 mr-1 pl-2 pr-2 bg-[rgba(0.2,0.2,0.2)] rounded-tl-lg rounded-br-lg">Features</a>
        <a href="/guide/how-to-get-started" class="mx-auto mb-2 md:mb-0 text-[white] hover:text-[red] hover:bg-[rgba(0,0,0,0.8)] ml-1 mr-1 pl-2 pr-2 bg-[rgba(0.2,0.2,0.2)] rounded-tl-lg rounded-br-lg">Download</a>
       </nav>

      <br>

      <router-outlet></router-outlet>

      <br>

      <footer class="text-sm select-none mx-auto">
        <img class="mx-auto" src="https://hits.seeyoufarm.com/api/count/keep/badge.svg?url=https%3A%2F%2Fgithub.com%2FRiddleTime%2FRace-Element&amp;count_bg=%23FF4500&amp;title_bg=%23555555&amp;icon=&amp;icon_color=%23E7E7E7&amp;title=Usage%3A+Today+%2F+All-time&amp;edge_flat=false">
        <p class="text-center">Copyright Reinier Klarenberg 2024</p>
      </footer>
    </div>
  `,
  styles: [
    `
    `,
  ],
})
export class AppComponent {


}
