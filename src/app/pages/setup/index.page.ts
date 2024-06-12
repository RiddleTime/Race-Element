import { RouteMeta } from '@analogjs/router';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

export const routeMeta: RouteMeta = {
  meta: [
    {
      property: 'og:title',
      content: 'Race Element - SetupLink',
    },
  ],
};

@Component({
  standalone: true,
  template: `
  <div class="container mx-auto text-center select-none">
    <h2>Opening Race Element's Setup Importer
    </h2>

    </div>
  `,
  imports: []
})
export default class SetupLinkPage implements OnInit {

  // https://race.elementfuture.com/setup?....

  setupLink: string = '';
  constructor(private route: ActivatedRoute) {

  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params: { [x: string]: any; }) => {
      let link = params['link'];
      if (link !== undefined) {
        this.setupLink = link;
        console.log(this.setupLink);
        window.location.assign('RaceElement://setup=' + this.setupLink);
      }
    });
  }
}
