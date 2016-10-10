import React from 'react'
import ReactDOM from 'react-dom'
import classNames from 'classnames'
import 'whatwg-fetch'
import LoadingIndicator from './loadingindicator.jsx'
import ArtistView from './artistview.jsx'

const defaultState = {
    user: {},
    artists: {},
    artistOrder: [],
    duration: 'medium-term',
    isLoading: true
}

class App extends React.Component {
    constructor(props) {
        super(props);
        this.state = defaultState;
        this.fetchData = this.fetchData.bind(this);
    }
    fetchData() {
        fetch('/api/artists', { credentials: 'same-origin' })
            .then((response) => { return response.json(); })
            .then((json) => {
                console.dir(json);
            });
    }
    componentDidMount() {
        this.fetchData();
    }
    render() {
        var orderedArtists = this.state.artistOrder.map(id => this.state.artists[id]);
        return (
            <div className="container">
                <LoadingIndicator display={this.state.isLoading} />
                <ArtistView display={!this.state.isLoading} artists={orderedArtists} />
            </div>
        )
    }
}

ReactDOM.render(
    <App/>, 
    document.getElementById('app')
);